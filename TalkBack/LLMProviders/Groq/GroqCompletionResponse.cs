using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Groq;

internal class GroqCompletionsResponse
{
    [JsonPropertyName("id")]
    public string? Id { get; set; }

    [JsonPropertyName("object")]
    public string? Object { get; set; }

    [JsonPropertyName("created")]
    public int Created { get; set; }

    [JsonPropertyName("model")]
    public string? Model { get; set; }

    [JsonPropertyName("choices")]
    public GroqChoice[]? Choices { get; set; }
}

internal class GroqChoice
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("index")]
    public int Index { get; set; }

    [JsonPropertyName("logprobs")]
    public object? Logprobs { get; set; }

    [JsonPropertyName("finish_reason")]
    public string? FinishReason { get; set; }

    [JsonPropertyName("delta")]
    public GroqDelta? Delta { get; set; }

    [JsonPropertyName("message")]
    public GroqConversationItem? Message { get; set; }
}

internal class GroqDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
