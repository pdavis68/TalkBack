using TalkBack.Models;

namespace TalkBack.Interfaces;
public interface IConversationContext
{
    string SystemPrompt { get; set; }
    void SetCompletionCallback(ICompletionCallback completionCallback);
    IEnumerable<ConversationItem> GetConverstationHistory();
}
