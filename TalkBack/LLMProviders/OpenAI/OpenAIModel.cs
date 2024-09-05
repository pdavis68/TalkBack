using System.Text.Json.Serialization;
using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.OpenAI;

public class OpenAIModel : ILLMModel
{
    [JsonPropertyName("id")]
    public string? Name { get; set; }

    [JsonPropertyName("object")]
    public string? Description { get; set; }

    [JsonPropertyName("owned_by")]
    public string? Owner { get; set; }

    public int ContextWindow { get; set; }

    public bool SupportsImages { get; set; } = false;
}
