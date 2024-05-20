namespace TalkBack.Interfaces;

public interface ILLMProvider
{
    void InitProvider(IProviderOptions? options);

    public string Name { get; }

    bool SupportsStreaming { get; }

    public IConversationContext CreateNewContext(string? systemPrompt = null);

    Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context);
    Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context);
}
