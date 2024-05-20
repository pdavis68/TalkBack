using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Claude;

internal class ClaudeStreamingData
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }
    [JsonPropertyName("index")]
    public int? Index { get; set; }
    [JsonPropertyName("delta")]
    public ClaudeContentDelta? Delta{ get; set; }
}
