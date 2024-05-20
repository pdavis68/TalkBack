using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Ollama;

internal class OllamaContext : IConversationContext
{
    public int[]? ContextData { get; set; }
    public List<ConversationItem> Conversation { get; set; } = new List<ConversationItem>();
    public string SystemPrompt { get; set; } = string.Empty;
    public string PartialResponse { get; set; } = string.Empty;
    public string CurrentPrompt { get; set; } = string.Empty;

    internal ICompletionCallback? CompletionCallback { get; set; } = null;
    public void SetCompletionCallback(ICompletionCallback completionCallback)
    {
        CompletionCallback = completionCallback;
    }

    public IEnumerable<IConversationItem> GetConverstationHistory()
    {
        return Conversation;
    }

    public class ConversationItem : IConversationItem
    {
        public string User { get; set; } = string.Empty;
        public string Assistant { get; set; } = string.Empty;
    }
}
