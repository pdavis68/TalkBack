using Microsoft.Extensions.Logging;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;
using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.Models;

namespace TalkBack.LLMProviders.OpenAI;

/// <summary>
/// OpenAI API docs: https://platform.openai.com/docs/api-reference
/// </summary>
public class OpenAIProvider : ILLMProvider
{
    private const string SYSTEM = "system";
    private const string USER = "user";
    private const string ASSISTANT = "assistant";

    private readonly IHttpHandler _httpHandler;
    private readonly ILogger _logger;
    private OpenAIOptions? _options;

    public OpenAIProvider(ILogger<OpenAIProvider> logger, IHttpHandler httpHandler)
    {
        _logger = logger;
        _httpHandler = httpHandler;
    }
    public string Name => "OpenAI";

    public string Version => "1.0.7";

    public bool SupportsStreaming => true;

    // Constructor and properties

    public async Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null, List<ImageUrl>? imageUrls = null)
    {
        if (context is null)
        {
            context = new OpenAIContext();
        }
        var ocontext = context as OpenAIContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }
        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
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
            throw new InvalidOperationException($"Failure calling OpenAI completions endpoint. Status Code: {response.StatusCode}");
        }

        var result = await response.Content.ReadAsStringAsync();
        var completion = JsonSerializer.Deserialize<OpenAICompletionsResponse>(result);
        if (completion is null)
        {
            throw new InvalidOperationException("Completion was null");
        }
        if (completion.Choices is not null && completion.Choices.Length > 0)
        {
            var responseText = completion.Choices[0].Message?.Content ?? string.Empty;
            ocontext.Conversation.Add(new ConversationItem() { User = prompt, Assistant = responseText });
            return new OpenAIResponse() { Response = responseText, Context = context };
        }
        _logger.LogError("Completion had no choices.");
        throw new InvalidOperationException("Completion had no choices.");
    }

    public void InitProvider(IProviderOptions? options)
    {
        _logger.LogDebug("Initializing OllamaPlugin with provided options.");
        if (options is null || options is not OpenAIOptions || string.IsNullOrEmpty((options as OpenAIOptions)!.Model))
        {
            _options = null;
            throw new InvalidOptionsException("The OpenAi Plugin requires an instance of the OpenAIOptions class with a valid Model set.");
        }
        _options = options as OpenAIOptions;
    }

    public async Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null, List<ImageUrl>? imageUrls = null)
    {
        _httpHandler.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("text/event-stream"));

        if (context is null)
        {
            context = new OpenAIContext();
        }
        var ocontext = context as OpenAIContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }
        var content = JsonSerializer.Serialize(new
        {
            model = _options!.Model,
            messages = BuildPrompt(prompt, context),
            stream = true
        }).Replace(",\"image_url\":null", "");


        var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
        {
            Content = new StringContent(content , Encoding.UTF8, "application/json"),
        };
        request.Headers.Add("Authorization", $"Bearer {_options.ApiKey}");
        var req = await request.Content.ReadAsStringAsync();
        var response = await _httpHandler.SendAsync(request, HttpCompletionOption.ResponseHeadersRead);

        // Set current prompt and partial response
        var oContext = (OpenAIContext)context;
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
                var eventResponse = JsonSerializer.Deserialize<OpenAICompletionsResponse>(line);
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
                    await receiver.ReceiveCompletionPartAsync(new OpenAIResponse { Response = oContext.PartialResponse, Context = context }, true);
                    break;
                }
                else
                {
                    await receiver.ReceiveCompletionPartAsync(new OpenAIResponse { Response = eventResponse.Choices[0].Delta!.Content, Context = context }, false);
                }
            }
        }
        else
        {
            _logger.LogError($"HTTP POST request failed with status code: {response.StatusCode}");
            throw new HttpRequestException($"HTTP POST request failed with status code: {response.StatusCode}");
        }
    }

    private List<OpenAIConversationItem> BuildPrompt(string prompt, IConversationContext? context)
    {
        var conversation = new List<OpenAIConversationItem>();
        var ocontext = context as OpenAIContext;
        if (ocontext is null)
        {
            throw new ArgumentException("Invalid context provided");
        }

        if (_options is not null && !string.IsNullOrWhiteSpace(ocontext.SystemPrompt))
        {
            conversation.Add(new OpenAIConversationItem(SYSTEM, ContentFromString(ocontext.SystemPrompt)));
        }
        foreach(var conversationItem in ocontext.Conversation)
        {
            if (!string.IsNullOrWhiteSpace(conversationItem.User))
            {
                if (conversationItem.ImageUrls is not null && conversationItem.ImageUrls.Count > 0)
                {
                    conversation.Add(new OpenAIConversationItem(USER, ContentFromStringAndImages(conversationItem.User, conversationItem.ImageUrls)));
                }
                else
                {
                    conversation.Add(new OpenAIConversationItem(USER, ContentFromString(conversationItem.User)));
                }
                conversation.Add(new OpenAIConversationItem(USER, ContentFromString(conversationItem.User)));
                conversation.Add(new OpenAIConversationItem(ASSISTANT, ContentFromString(conversationItem.Assistant ?? string.Empty)));
            }
        }
        conversation.Add(new OpenAIConversationItem(USER, ContentFromString(prompt)));
        return conversation;
    }

    private List<ContentItem> ContentFromString(string text)
    {
        return new List<ContentItem>() { new ContentItem { Type = "text", Text  = text} };
    }
    private List<ContentItem> ContentFromStringAndImages(string text, List<ImageUrl> imageUrls)
    {
        var content = ContentFromString(text);
        foreach (var imageUrl in imageUrls)
        {
            content.Add(new ContentItem { Type = "image_url", ImageUrl = new ImageUrl() { Url = imageUrl.Url, Detail = imageUrl.Detail } });
        }
        return content;
    }


    private OpenAIResponse BuildOpenAIResponse(string prompt, OpenAICompletionsResponse result, IConversationContext? context)
    {
        var openAIResponse = new OpenAIResponse
        {
            Response = result.Choices![0].Text,
            Context = context as OpenAIContext
        };

        (openAIResponse.Context as OpenAIContext)!.Conversation.Add(new ConversationItem
        {
            User = prompt,
            Assistant = openAIResponse?.Response ?? ""
        });

        return openAIResponse!;
    }

    public IConversationContext CreateNewContext(string? systemPrompt = null, List<ConversationItem>? conversation = null)
    {
        return new OpenAIContext()
        {
            SystemPrompt = systemPrompt ?? string.Empty,
            Conversation = conversation ?? new List<ConversationItem>()
        };
    }

    public async Task<List<ILLMModel>> GetModelsAsync()
    {
        var request = new HttpRequestMessage(HttpMethod.Get, "https://api.openai.com/v1/models");
        request.Headers.Add("Authorization", $"Bearer {_options!.ApiKey}");
        var response = await _httpHandler.SendAsync(request, HttpCompletionOption.ResponseContentRead);
        if (!response.IsSuccessStatusCode)
        {
            throw new InvalidOperationException($"Failure calling OpenAI models endpoint. Status Code: {response.StatusCode}");
        }
        var modelList = await response.Content.ReadFromJsonAsync<OpenAIModelList>();
        if (modelList is null)
        {
            throw new InvalidOperationException("Model list was null");
        }
        return modelList.Data.Select(m => PreprocessModel(m)).ToList<ILLMModel>();
    }

    private OpenAIModel PreprocessModel(OpenAIModel m)
    {
        switch(m.Name)
        {
            case "gpt-4o":
            case "gpt-4o-mini":
            case "gpt-4o-mini-2024-07-18":
            case "gpt-4o-2024-05-13":
            case "gpt-4o-2024-08-06":
            case "chatgpt-4o-latest":
            case "gpt-4-turbo":
            case "gpt-4-turbo-2024-04-09":
            case "gpt-4-turbo-preview":
            case "gpt-4-0125-preview":
            case "gpt-4-1106-preview":
                m.ContextWindow = 128000;
                m.SupportsImages = true;
                break;
            case "gpt-4":
            case "gpt-4-0613":
            case "gpt-4-0314":
                m.ContextWindow = 8192;
                m.SupportsImages = true;
                break;
            case "gpt-3.5-turbo-0125":
            case "gpt-3.5-turbo":
            case "gpt-3.5-turbo-1106":
                m.ContextWindow = 16385;
                m.SupportsImages = false;
                break;
            default:
                m.ContextWindow = 4096;
                m.SupportsImages = false;
                break;
        }
        return m;
    }
}
