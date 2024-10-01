using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.OpenAI;


public class OpenAICompletionsResponse
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
    public OpenAIChoice[]? Choices { get; set; }

}

public class OpenAIChoice
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
    public OpenAIDelta? Delta { get; set; }

    [JsonPropertyName("message")]
    public OpenAIReceivedMessage? Message { get; set; }

}

public class OpenAIDelta
{
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}