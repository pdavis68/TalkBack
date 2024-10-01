using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Claude;

public class ClaudeResponse : IModelResponse
{
    public IConversationContext? Context { get; set; }

    public string Response { get; set; } = string.Empty;
}
