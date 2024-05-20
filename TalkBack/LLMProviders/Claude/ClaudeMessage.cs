using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;
    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
