using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.OpenAI;

internal class OpenAIResponse : IModelResponse
{
    public string? Response { get; set; }
    public IConversationContext? Context { get; set; }
}
