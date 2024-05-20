using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using TalkBack.Interfaces;

namespace TalkBack;

public class ProviderActivator : IProviderActivator
{
    private readonly ILogger _logger;
    private readonly IServiceProvider _serviceProvider;

    public ProviderActivator(ILogger<ProviderActivator> logger, IServiceProvider serviceProvider)
    {
        _logger = logger;
        _serviceProvider = serviceProvider;
    }

    public ILLMProvider? CreateProvider<T>() where T : ILLMProvider
    {
        _logger.LogDebug($"Create provider for {typeof(T).Name}");
        try
        {
            return _serviceProvider.GetService<T>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error creating instance of LLM provider type {typeof(T).Name}");
            throw;
        }
    }

}
