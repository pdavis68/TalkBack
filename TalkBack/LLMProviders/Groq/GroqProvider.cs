using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.OpenAI;
using TalkBack.Models;

namespace TalkBack.LLMProviders.Groq;

public class GroqProvider : ILLMProvider
{
    private const string SYSTEM = "system";
    private const string USER = "user";
    private const string ASSISTANT = "assistant";

    private readonly IHttpHandler _httpHandler;
    private readonly ILogger _logger;
    private GroqOptions? _options;

    public GroqProvider(ILogger<GroqProvider> logger, IHttpHandler httpHandler)
    {
        _logger = logger;
        _httpHandler = httpHandler;
    }

    public string Name => "Groq";

    public string Version => "1.0.0"; // Placeholder version

    public bool SupportsStreaming => true;

    // Constructor and properties

    public async Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null, List<ImageUrl>? imageUrls = null)
    {
        if (context is null)
        {
            context = new GroqContext();
        }
        var ocontext = context as GroqContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = _options!.Model,
                messages = BuildPrompt(prompt, context),
                stream = false
            }), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        var req = await request.Content.ReadAsStringAsync();
        using var response = await _httpHandler.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failure calling Groq completions endpoint. Status Code: {response.StatusCode}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var completion = JsonSerializer.Deserialize<GroqCompletionsResponse>(result);
        if (completion is null)
        {
            throw new InvalidOperationException("Completion was null");
        }
        if (completion.Choices is not null && completion.Choices.Length > 0)
        {
            var responseText = completion.Choices[0].Message?.Content ?? string.Empty;
            ocontext.Conversation.Add(new ConversationItem() { User = prompt, Assistant = responseText });
            return new GroqResponse() { Response = responseText, Context = context };
        }
        _logger.LogError("Completion had no choices.");
        throw new InvalidOperationException("Completion had no choices.");
    }

    public void InitProvider(IProviderOptions? options)
    {
        _logger.LogDebug("Initializing OllamaPlugin with provided options.");
        if (options is null || options is not GroqOptions || string.IsNullOrEmpty((options as GroqOptions)!.Model))
        {
            _options = null;
            throw new InvalidOptionsException("The Groq Plugin requires an instance of the GroqOptions class with a valid Model set.");
        }
        _options = options as GroqOptions;
    }

    public async Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null, List<ImageUrl>? imageUrls = null)
    {
        _httpHandler.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        if (context is null)
        {
            context = new GroqContext();
        }
        var ocontext = context as GroqContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.groq.com/openai/v1/chat/completions")
        {
            Content = new StringContent(JsonSerializer.Serialize(new
            {
                model = _options!.Model,
                messages = BuildPrompt(prompt, context),
                stream = true
            }), Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        var req = await request.Content.ReadAsStringAsync();
        var response = await _httpHandler.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Set current prompt and partial response
        var oContext = (GroqContext)context;
        oContext.CurrentPrompt = prompt;
        oContext.PartialResponse = string.Empty;

        if (response.IsSuccessStatusCode)
        {
            // Read the SSE stream from the response
            var sseStream = await response.Content.ReadAsStreamAsync();

            // Handling of the SSE stream
            using var reader = new StreamReader(sseStream, Encoding.UTF8);
            while (!reader.EndOfStream)
            {
                string? line = await reader.ReadLineAsync();
                if (line == null || line == "data:[DONE]")
                {
                    break;
                }
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }
                if (line.StartsWith("data:"))
                {
                    line = line.Substring(5);
                }

                // Deserialize the event data
                var eventResponse = JsonSerializer.Deserialize<GroqCompletionsResponse>(line);
                if (eventResponse == null ||
                    eventResponse.Choices is null ||
                    eventResponse.Choices.Length == 0 ||
                    eventResponse.Choices[0].Delta is null ||
                    (string.IsNullOrWhiteSpace(eventResponse.Choices[0].Delta!.Content) && string.IsNullOrWhiteSpace(eventResponse.Choices[0].FinishReason)))
                {
                    continue;
                }

                // Append to partial response
                oContext.PartialResponse += eventResponse.Choices![0].Delta!.Content;

                // Update conversation and call completion callback if done
                if (eventResponse.Choices[0].FinishReason == "stop")
                {
                    oContext.Conversation.Add(new ConversationItem { User = prompt, Assistant = oContext.PartialResponse });
                    await receiver.ReceiveCompletionPartAsync(new GroqResponse { Response = oContext.PartialResponse, Context = context }, true);
                    break;
                }
                else
                {
                    await receiver.ReceiveCompletionPartAsync(new GroqResponse { Response = eventResponse.Choices[0].Delta!.Content, Context = context }, false);
                }
            }
        }
        else
        {
            _logger.LogError($"HTTP POST request failed with status code: {response.StatusCode}");
            throw new HttpRequestException($"HTTP POST request failed with status code: {response.StatusCode}");
        }
    }

    private List<GroqConversationItem> BuildPrompt(string prompt, IConversationContext? context)
    {
        var conversation = new List<GroqConversationItem>();
        var ocontext = context as GroqContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }

        if (_options is not null && !string.IsNullOrWhiteSpace(ocontext.SystemPrompt))
        {
            conversation.Add(new GroqConversationItem(SYSTEM, ocontext.SystemPrompt));
        }
        foreach (var conversationItem in ocontext.Conversation)
        {
            if (!string.IsNullOrWhiteSpace(conversationItem.User))
            {
                conversation.Add(new GroqConversationItem(USER, conversationItem.User));
                conversation.Add(new GroqConversationItem(ASSISTANT, conversationItem.Assistant ?? string.Empty));
            }
        }
        conversation.Add(new GroqConversationItem(USER, prompt));
        return conversation;
    }

    private GroqResponse BuildGroqResponse(string prompt, GroqCompletionsResponse result, IConversationContext? context)
    {
        var GroqResponse = new GroqResponse
        {
            Response = result.Choices![0].Text,
            Context = context as GroqContext
        };

        (GroqResponse.Context as GroqContext)!.Conversation.Add(new ConversationItem
        {
            User = prompt,
            Assistant = GroqResponse?.Response ?? ""
        });

        return GroqResponse!;
    }

    public IConversationContext CreateNewContext(string? systemPrompt = null)
    {
        return new GroqContext()
        {
            SystemPrompt = systemPrompt ?? string.Empty
        };
    }

    public async Task<List<ILLMModel>> GetModelsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.groq.com/openai/v1/models");
        request.Headers.Add("Authorization", $"Bearer {_options!.ApiKey}");
        var response = await _httpHandler.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failure calling OpenAI models endpoint. Status Code: {response.StatusCode}");
        }
        var modelList = await response.Content.ReadFromJsonAsync<GroqModelList>();
        if (modelList is null)
        {
            throw new InvalidOperationException("Model list was null");
        }
        return modelList.Data.ToList<ILLMModel>();
    }
}
