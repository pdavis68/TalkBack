using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.Models;
using Microsoft.Extensions.Logging;

namespace TalkBack.LLMProviders.Ollama;

public class OllamaProvider : ILLMProvider
{
    private readonly ILogger<OllamaProvider> _logger;
    private readonly IHttpHandler _httpHandler;
    private OllamaOptions? _options;

    /// <summary>
    /// Ollama API docs: https://github.com/jmorganca/ollama/blob/main/docs/api.md
    /// </summary>
    public OllamaProvider(ILogger<OllamaProvider> logger, IHttpHandler httpHandler)
    {
        _logger = logger;
        _httpHandler = httpHandler;
    }

    public bool SupportsStreaming => true;

    public string Name => "Ollama";

    public void InitProvider(IProviderOptions? options)
    {
        _logger.LogDebug("Initializing OllamaPlugin with provided options.");
        _options = options as OllamaOptions;
        if (_options is null || string.IsNullOrEmpty(_options.Model) || string.IsNullOrEmpty(_options.ServerUrl))
        {
            _options = null;
            throw new InvalidOptionsException("The Ollama Plugin requires an instance of the OllamaOptions class with a valid Model and ServerUrl!");
        }
    }

    public async Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("You must Init the model first.");
        }
        if (context is not null && context is not OllamaContext)
        {
            throw new InvalidConversationContextException("Received an invalid context.");
        }
        var newPrompt = GeneratePrompt(context, prompt);
        OllamaParameters parameters = GenerateParameters(newPrompt, context as OllamaContext, false);
        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpHandler.PostAsync(_options.ServerUrl + "/generate", content);
        var result = await response.Content.ReadAsStringAsync();

        if (context is null)
        {
            context = new OllamaContext();
        }
        var responseOb = JsonSerializer.Deserialize<OllamaCompletionResponse>(result);

        (context as OllamaContext)!.ContextData = responseOb?.Context;
        if ((context as OllamaContext)!.CompletionCallback is not null)
        {
            (context as OllamaContext)!.CompletionCallback!.Complete(Name, $"Model: {parameters.Model}", prompt, responseOb?.Response ?? string.Empty);
        }
        return new OllamaResponse()
        {
            Response = responseOb?.Response ?? string.Empty,
            Context = context
        };
    }

    public async Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("You must Init the model first.");
        }
        if (context is not null && context is not OllamaContext)
        {
            throw new InvalidConversationContextException("Received an invalid context.");
        }
        OllamaParameters parameters = GenerateParameters(prompt, context as OllamaContext, true);
        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpHandler.PostAsync(_options.ServerUrl + "/generate", content);
        if (context is null)
        {
            context = new OllamaContext();
        }
        (context as OllamaContext)!.CurrentPrompt = GeneratePrompt(context, prompt);
        (context as OllamaContext)!.PartialResponse = string.Empty;

        if (response.IsSuccessStatusCode)
        {
            // Read the SSE stream from the response.
            var sseStream = await response.Content.ReadAsStreamAsync();

            // Create an observable sequence from the SSE stream.
            var sseObservable = Observable.Using(
                () => new StreamReader(sseStream, Encoding.UTF8),
                streamReader => Observable.Create<string>(
                    async (observer, cancellationToken) =>
                    {
                        try
                        {
                            while (!cancellationToken.IsCancellationRequested)
                            {
                                string? line = await streamReader.ReadLineAsync();
                                if (line == null)
                                    break;

                                observer.OnNext(line);
                            }
                            observer.OnCompleted();
                        }
                        catch (Exception ex)
                        {
                            observer.OnError(ex);
                        }
                    }));

            // Subscribe to the SSE events.
            var subscription = sseObservable.Subscribe(eventData =>
            {
                var response = JsonSerializer.Deserialize<OllamaCompletionResponse>(eventData);
                if (response is null)
                {
                    return;
                }
                (context as OllamaContext)!.PartialResponse += response.Response;
                if (response.Done)
                {
                    (context as OllamaContext)!.Conversation.Add(new ConversationItem()
                    {
                        Assistant = (context as OllamaContext)!.PartialResponse,
                        User = (context as OllamaContext)!.CurrentPrompt
                    });

                    if ((context as OllamaContext)!.CompletionCallback is not null)
                    {
                        (context as OllamaContext)!.CompletionCallback!.Complete(Name, $"Model: {parameters.Model}", (context as OllamaContext)!.CurrentPrompt, (context as OllamaContext)!.PartialResponse);
                    }
                    (context as OllamaContext)!.ContextData = response.Context;
                }
                receiver.ReceiveCompletionPartAsync(new OllamaResponse()
                {
                    Context = context,
                    Response = response.Response ?? ""
                }, response.Done);
            });
        }
        else
        {
            Console.WriteLine("HTTP POST request failed with status code: " + response.StatusCode);
        }
    }

    private string GeneratePrompt(IConversationContext? context, string prompt)
    {
        var ctxt = (context as OllamaContext)!;
        string newPrompt = string.Empty;
        if (ctxt.Conversation.Count > 0)
        {
            newPrompt = "You are 'Assistant'. This is the conversation so far between the user, and you;\n\n";
            foreach (var item in ctxt.Conversation)
            {
                newPrompt += $"User: {item.User} {Environment.NewLine}";
                newPrompt += $"Assistant: {item.Assistant} {Environment.NewLine}";
            }
        }

        newPrompt += "User: " + prompt;
        return newPrompt;
    }

    private OllamaParameters GenerateParameters(string prompt, OllamaContext? context, bool streaming)
    {
        return new OllamaParameters()
        {
            Prompt = prompt,
            Context = context?.ContextData,
            Stream = streaming,
            Model = _options!.Model,
            Seed = _options.Seed,
            FrequencyPenalty = _options.FrequencyPenalty,
            NumPredict = _options.NumPredict,
            PresencePenalty = _options.PresencePenalty,
            RepeatPenalty = _options.RepeatPenalty,
            Temperature = _options.Temperature,
            TopK = _options.TopK,
            TopP = _options.TopP
        };
    }

    public IConversationContext CreateNewContext(string? systemPrompt = null)
    {
        return new OllamaContext()
        {
            SystemPrompt = systemPrompt ?? string.Empty
        };
    }
}
