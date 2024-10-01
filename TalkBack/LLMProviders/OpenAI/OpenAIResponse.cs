using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.OpenAI;

public class OpenAIResponse : IModelResponse
{
    public string? Response { get; set; }
    public IConversationContext? Context { get; set; }
}
