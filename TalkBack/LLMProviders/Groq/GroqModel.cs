using System.Text.Json.Serialization;
using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Groq;

public class GroqModel : ILLMModel
{
    [JsonPropertyName("id")]
    public string? Name { get; set; }
    [JsonPropertyName("object")]
    public string? Description { get; set; }
    [JsonPropertyName("owned_by")]
    public string? Owner { get; set; }
    [JsonPropertyName("context_window")]
    public int ContextWindow { get; set; }
    public bool SupportsImages { get; set; } = false;
}

