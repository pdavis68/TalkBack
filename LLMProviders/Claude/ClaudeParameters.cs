using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeParameters
{
    [JsonPropertyName("model")]
    public string? Model { get; set; }
    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 4096;
    // Optional options
    [JsonPropertyName("stream")]
    public bool Stream { get; set; } = false;
    [JsonPropertyName("messages")]
    public List<ClaudeMessage> Messages { get; set; } = new List<ClaudeMessage>();
}
