using TalkBack.Interfaces;
using TalkBack.Models;

namespace TalkBack.LLMProviders.Groq;

public class GroqContext : IConversationContext
{
    public List<ConversationItem> Conversation { get; set; } = new List<ConversationItem>();
    public string SystemPrompt { get; set; } = string.Empty;
    public string PartialResponse { get; set; } = string.Empty;
    public string CurrentPrompt { get; set; } = string.Empty;

    internal ICompletionCallback? CompletionCallback { get; set; } = null;
    public void SetCompletionCallback(ICompletionCallback completionCallback)
    {
        CompletionCallback = completionCallback;
    }

    public IEnumerable<ConversationItem> GetConverstationHistory()
    {
        return Conversation;
    }
}
