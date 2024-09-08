using TalkBack.Models;

namespace TalkBack.Interfaces;

public interface ILLMProvider
{
    public string Name { get; }
    bool SupportsStreaming { get; }

    void InitProvider(IProviderOptions? options);
    public IConversationContext CreateNewContext(string? systemPrompt = null, List<ConversationItem>? conversation = null);
    Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context, List<ImageUrl>? imageUrls = null);
    Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context, List<ImageUrl>? imageUrls = null);
    Task<List<ILLMModel>> GetModelsAsync();
}
