using TalkBack.Interfaces;
using TalkBack.Models;

namespace TalkBack.LLMProviders.Claude;

internal class ClaudeContext : IConversationContext
{
    public List<ConversationItem> ContextData { get; set; } = new List<ConversationItem>();
    public string SystemPrompt { get; set; } = string.Empty;
    public string PartialResponse { get; set; } = string.Empty;
    public string CurrentPrompt { get; set; } = string.Empty;

    internal ICompletionCallback? CompletionCallback { get; set; } = null;

    public IEnumerable<ConversationItem> GetConverstationHistory()
    {
        return ContextData;
    }

    public void SetCompletionCallback(ICompletionCallback completionCallback)
    {
        CompletionCallback = completionCallback;
    }

}
