namespace TalkBack.Interfaces;

public interface ILLM
{
    void SetProvider(ILLMProvider provider);
    IConversationContext? CreateNewContext();
    Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null);
    Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null);
}
