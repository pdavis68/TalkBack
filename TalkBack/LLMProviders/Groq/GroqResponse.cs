using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Groq;

public class GroqResponse : IModelResponse
{
    public string? Response { get; set; }
    public IConversationContext? Context { get; set; }
}
