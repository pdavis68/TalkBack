using TalkBack.Exceptions;
using TalkBack.Interfaces;
using Microsoft.Extensions.Logging;
using TalkBack.Models;

namespace TalkBack;

public class LLM : ILLM
{
    private readonly ILogger<LLM> _logger;
    private ILLMProvider? _selectedProvider;

    public LLM(ILogger<LLM> logger)
    {
        _logger = logger;
    }

    public ILLMProvider? Provider => _selectedProvider;

    public void SetProvider(ILLMProvider provider)
    {
        _logger.LogDebug($"SetProvider to: {provider.Name}");
        _selectedProvider = provider;
    }

    public IConversationContext? CreateNewContext(List<ConversationItem>? conversation = null)
    {
        _logger.LogDebug("CreateNewContext");
        EnsureProvider();
        return _selectedProvider!.CreateNewContext();
    }

    public async Task StreamCompletionAsync(ICompletionReceiver receiver, string prompt, IConversationContext? context = null)
    {
        _logger.LogDebug($"StreamCompletionAsync - {prompt}");
        EnsureProvider();
        await _selectedProvider!.StreamCompletionAsync(receiver, prompt, context);
    }

    public async Task<IModelResponse> CompleteAsync(string prompt, IConversationContext? context = null)
    {
        _logger.LogDebug($"CompleteAsync - {prompt}");
        EnsureProvider();
        return await _selectedProvider!.CompleteAsync(prompt, context);
    }

    private void EnsureProvider()
    {
        if (_selectedProvider == null)
        {
            throw new NoProviderSetException();
        }
    }
}