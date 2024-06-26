using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.Models;
using Microsoft.Extensions.Logging;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using System.Diagnostics;

namespace TalkBack.LLMProviders.Claude;

/// <summary>
/// Claude API docs: https://docs.anthropic.com/claude/reference/getting-started-with-the-api
/// </summary>
public class ClaudeProvider : ILLMProvider
{
    private readonly ILogger<ClaudeProvider> _logger;
    private readonly IHttpHandler _httpHandler;
    private ClaudeOptions? _options;

    public ClaudeProvider(ILogger<ClaudeProvider> logger, IHttpHandler httpHandler)
    {
        _logger = logger;
        _httpHandler = httpHandler;
    }

    public bool SupportsStreaming => true;

    public string Name => "Claude";

    public void InitProvider(IProviderOptions? options)
    {
        _logger.LogDebug("Initializing ClaudeProvider with provided options.");
        if (_options != null)
        {
            throw new InvalidOptionsException("You can only initialize the ClaudeProvider once!");
        }
        _options = options as ClaudeOptions;
        if (_options is null)
        {
            _options = null;
            throw new InvalidOptionsException("The Claude Provider requires an instance of the ClaudeOptions class with a valid Model!");
        }
        if (string.IsNullOrEmpty(_options.Model))
        {
            throw new InvalidOptionsException("The Claude Provider requires an instance of the ClaudeOptions class with a valid Model!");
        }
        _httpHandler.DefaultRequestHeaders.Add("anthropic-version", _options.AnthropicVersion);
        _httpHandler.DefaultRequestHeaders.Add("x-api-key", _options.ApiKey);
    }

    public async Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("You must Init the provider first.");
        }
        if (context is null)
        {
            context = new ClaudeContext();
        }
        if (context is null || context is not ClaudeContext)
        {
            throw new InvalidConversationContextException("Received an invalid context.");
        }
        var messages = GenerateMessages(context, prompt);
        ClaudeParameters parameters = GenerateParameters(messages, context as ClaudeContext, false);
        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        var response = await _httpHandler.PostAsync("https://api.anthropic.com/v1/messages", content);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"{response.StatusCode} - {response.ReasonPhrase}");
        }
        var result = await response.Content.ReadAsStringAsync();

        if (context is null)
        {
            context = new ClaudeContext();
        }
        var responseOb = JsonSerializer.Deserialize<ClaudeMessageResponse>(result);

        if ((context as ClaudeContext)!.CompletionCallback is not null)
        {
            (context as ClaudeContext)!.CompletionCallback!.Complete(Name, $"Model: {parameters.Model}", prompt, responseOb?.Content?[0].Text ?? string.Empty);
        }
        (context as ClaudeContext)!.ContextData.Add(new ConversationItem { User = prompt, Assistant = responseOb!.Content?[0].Text ?? "" });

        return new ClaudeResponse()
        {
            Response = responseOb?.Content?[0].Text ?? string.Empty,
            Context = context
        };
    }

    public async Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null)
    {
        if (_options is null)
        {
            throw new InvalidOperationException("You must Init the model first.");
        }
        if (context is not null && context is not ClaudeContext)
        {
            throw new InvalidConversationContextException("Received an invalid context.");
        }
        var messages = GenerateMessages(context, prompt);
        ClaudeParameters parameters = GenerateParameters(messages, context as ClaudeContext, true);
        var jsonContent = JsonSerializer.Serialize(parameters);
        var content = new StringContent(jsonContent, Encoding.UTF8, "application/json");
        content.Headers.Add("anthropic-version", "2023-06-01");
        content.Headers.Add("x-api-key", _options.ApiKey);
        var response = await _httpHandler.PostAsync("https://api.anthropic.com/v1/messages", content);


        if (context is null)
        {
            context = new ClaudeContext();
        }
        (context as ClaudeContext)!.CurrentPrompt = prompt;
        (context as ClaudeContext)!.PartialResponse = string.Empty;

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

            String workingResponse = string.Empty;
            // Subscribe to the SSE events.
            var subscription = sseObservable.Subscribe(eventData =>
            {
                if (string.IsNullOrWhiteSpace(eventData) 
                    || eventData.StartsWith("event:"))
                {
                    return;
                }

                if (eventData.StartsWith("data: "))
                {
                    eventData = eventData.Substring(6);
                }
                var response = JsonSerializer.Deserialize<ClaudeStreamingData>(eventData);
                if (response is null)
                {
                    return;
                }
                bool final = false;
                if (response.Type != "content_block_delta")
                {
                    if (response.Type == "message_stop")
                    {
                        final = true;
                    }
                    else
                    {
                        Debug.WriteLine($"Response type: {response.Type}");
                        return;
                    }
                }

                var textResponse = response?.Delta?.Text ?? string.Empty;
                workingResponse += textResponse;
                (context as ClaudeContext)!.PartialResponse += textResponse;
                if (final)
                {
                    (context as ClaudeContext)!.ContextData.Add(new ConversationItem()
                    {
                        Assistant = workingResponse,
                        User = (context as ClaudeContext)!.CurrentPrompt
                    });
                    workingResponse = string.Empty;

                    if ((context as ClaudeContext)!.CompletionCallback is not null)
                    {
                        (context as ClaudeContext)!.CompletionCallback!.Complete(Name, $"Model: {parameters.Model}", (context as ClaudeContext)!.CurrentPrompt, (context as ClaudeContext)!.PartialResponse);
                    }
                }
                receiver.ReceiveCompletionPartAsync(new ClaudeResponse()
                {
                    Context = context,
                    Response = response?.Delta?.Text ?? ""
                }, final);
            });
        }
        else
        {
            Console.WriteLine("HTTP POST request failed with status code: " + response.StatusCode);
        }
    }

    private List<ClaudeMessage> GenerateMessages(IConversationContext? context, string prompt)
    {
        var messages = new List<ClaudeMessage>();

        var ctxt = (context as ClaudeContext)!;

        if (ctxt!.ContextData.Count > 0)
        {
            foreach (var item in ctxt.ContextData)
            {
                messages.Add(new ClaudeMessage()
                {
                    Role = "user",
                    Content = item.User
                });
                messages.Add(new ClaudeMessage()
                {
                    Role = "assistant",
                    Content = item.Assistant
                });
            }
        }

        messages.Add(new ClaudeMessage()
        {
            Role = "user",
            Content = prompt
        });
        messages.Add(new ClaudeMessage()
        {
            Role = "assistant",
            Content = string.Empty
        });
        return messages;
    }

    private ClaudeParameters GenerateParameters(List<ClaudeMessage> messages, ClaudeContext? context, bool streaming)
    {
        return new ClaudeParameters()
        {
            Messages = messages,
            Stream = streaming,
            Model = _options!.Model,
            MaxTokens = 4096
        };
    }

    public IConversationContext CreateNewContext(string? systemPrompt = null)
    {
        return new ClaudeContext()
        {
            SystemPrompt = systemPrompt ?? string.Empty
        };
    }
}
