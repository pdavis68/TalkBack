using Microsoft.Extensions.Logging;
using NSubstitute;
using TalkBack.LLMProviders.Claude;

namespace TalkBack.Test.LLMProviders.Claude;

public class ClaudeProviderTests
{
    private readonly ClaudeProvider _claudeProvider;


    private readonly IHttpClientFactory _httpClientFactory;
    private readonly HttpClient _httpClient;
    private readonly ILogger<ClaudeProvider>  _logger = Substitute.For<ILogger<ClaudeProvider>>();

    public ClaudeProviderTests()
    {
        
        _httpClientFactory = Substitute.For<IHttpClientFactory>();
        _httpClient = Substitute.For<HttpClient>();
        _httpClientFactory.CreateClient().Returns(_httpClient);
        _claudeProvider = new ClaudeProvider(_logger, _httpClientFactory);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claudeProvider.CompleteAsync("test prompt"));

        // Verify that InitProvider was not called
        _httpClientFactory.DidNotReceive().CreateClient();
    }

    [Fact]
    public async Task CompleteAsync_Success_WhenInitProviderCalled()
    {
        // Arrange
        var options = new ClaudeOptions()
        {
            Model = "test",
            AnthropicVersion = "2023-06-01",
            ApiKey = "key"
        };
        

        // Act & Assert
        _claudeProvider.InitProvider(options);
        var response = await _claudeProvider.CompleteAsync("test prompt");

        // Verify that HttpClient was configured correctly
        _httpClientFactory.Received().CreateClient();
        _httpClientFactory.ReceivedWithAnyArgs().CreateClient(Arg.Any<string>());

        // Additional assertions based on expected behavior of CompleteAsync
        // Note: This example assumes a successful completion, adjust according to your expectations
        Assert.NotNull(response);
    }

    // Add more tests here to cover other edge cases and behaviors of CompleteAsync
}
