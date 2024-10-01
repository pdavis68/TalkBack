using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeContentDelta
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;
    [JsonPropertyName("text")]
    public string Text { get; set; } = string.Empty;
}
