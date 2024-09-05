using System.Text.Json.Serialization;
using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Ollama;

public class OllamaModel : ILLMModel
{
    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("digest")]
    public string? Description { get; set; }

    public string? Owner { get; set; } = "ollama";

    public int ContextWindow { get; set; } = 4096; // default value. Who knows?

    public bool SupportsImages { get; set; } = false;
}
