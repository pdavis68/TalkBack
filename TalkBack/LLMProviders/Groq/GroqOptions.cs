using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Groq;

public class GroqOptions : IProviderOptions
{
    public string? ApiKey { get; set; }
    public string? Model { get; set; }
    public float Temperature { get; set; }
    public int MaxTokens { get; set; }
    public float TopP { get; set; }
    public float FrequencyPenalty { get; set; }
    public float PresencePenalty { get; set; }
    public string? Stop { get; set; }
}
