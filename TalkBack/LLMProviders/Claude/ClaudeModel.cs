using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeModel : ILLMModel
{
    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Owner { get; set; }

    public int ContextWindow { get; set; }

    public bool SupportsImages { get; set; }
}
