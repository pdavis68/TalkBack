using TalkBack.Models;

namespace TalkBack.Interfaces;

public interface ILLM
{
    ILLMProvider? Provider { get; }
    void SetProvider(ILLMProvider provider);
    IConversationContext? CreateNewContext(List<ConversationItem>? conversation = null);
    Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null);
    Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null);
}
