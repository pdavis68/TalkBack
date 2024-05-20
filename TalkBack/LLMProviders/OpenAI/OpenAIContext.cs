using Microsoft.VisualBasic;
using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.OpenAI;

internal class OpenAIContext : IConversationContext
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

    public IEnumerable<IConversationItem> GetConverstationHistory()
    {
        return Conversation;
    }

    public class ConversationItem : IConversationItem
    {
        public string? User { get; set; }
        public string? Assistant { get; set; }
    }
}