using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeMessageResponse
{
    [JsonPropertyName("content")]
    public ClaudeMessageItem[]? Content { get; set; }
    [JsonPropertyName("stop_reason")]
    public string? StopReason { get; set; }
    [JsonPropertyName("stop_sequence")]
    public string? StopSequence { get; set; }
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("id")]
    public string? Id { get; set; }
}

public class ClaudeMessageItem
{
    [JsonPropertyName("text")]
    public string? Text { get; set; }
    [JsonPropertyName("type")]
    public string? Type { get; set; }

}