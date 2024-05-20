using TalkBack.Interfaces;

namespace TalkBack.ModelPlugins.Claude;

internal class ClaudeContext : IConversationContext
{
    public List<ConversationItem> ContextData { get; set; } = new List<ConversationItem>();
    public string SystemPrompt { get; set; } = string.Empty;
    public string PartialResponse { get; set; } = string.Empty;
    public string CurrentPrompt { get; set; } = string.Empty;

    internal ICompletionCallback? CompletionCallback { get; set; } = null;

    public IEnumerable<IConversationItem> GetConverstationHistory()
    {
        return ContextData;
    }

    public void SetCompletionCallback(ICompletionCallback completionCallback)
    {
        CompletionCallback = completionCallback;
    }

    internal class ConversationItem : IConversationItem
    {
        internal string User { get; set; } = string.Empty;
        internal string Assistant { get; set; } = string.Empty;
    }
}
