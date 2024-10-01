using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Ollama;
using TalkBack.Models;

namespace TalkBack.Test.LLMProviders.Ollama;

public class OllamaProviderTests
{
    private readonly ILLMProvider _ollamaProvider;
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<OllamaProvider> _logger = Substitute.For<ILogger<OllamaProvider>>();

    public OllamaProviderTests()
    {
        _httpHandler = Substitute.For<IHttpHandler>();
        _ollamaProvider = new OllamaProvider(_logger, _httpHandler);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _ollamaProvider.CompleteAsync("test prompt", null));
    }

    [Fact]
    public async Task CompleteAsync_Success_WhenInitProviderCalled()
    {
        // Arrange
        var options = new OllamaOptions()
        {
            Model = "test-model",
            ServerUrl = "http://test-server"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _ollamaProvider.InitProvider(options);

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new OllamaCompletionResponse
            {
                Response = "Hello, this is a test response.",
                Context = new int[] { 1, 2, 3 }
            })
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var context = _ollamaProvider.CreateNewContext();
        var response = await _ollamaProvider.CompleteAsync("Test prompt", context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Hello, this is a test response.", response.Response);
        Assert.NotNull(response.Context);

        var ollamaContext = response.Context as OllamaContext;
        Assert.NotNull(ollamaContext);
        Assert.Equal(new int[] { 1, 2, 3 }, ollamaContext.ContextData);

        var conversationHistory = response.Context.GetConverstationHistory().ToList();
        Assert.Empty(conversationHistory); // Ollama doesn't update conversation history in CompleteAsync
    }

    [Fact]
    public async Task CompleteAsync_HandlesEmptyResponse()
    {
        // Arrange
        SetupProvider();

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new OllamaCompletionResponse
            {
                Response = string.Empty,
                Context = Array.Empty<int>()
            })
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _ollamaProvider.CompleteAsync("Test prompt", null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.Response);
    }

    [Fact]
    public async Task CompleteAsync_HandlesNullResponse()
    {
        // Arrange
        SetupProvider();

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new OllamaCompletionResponse
            {
                Response = null,
                Context = Array.Empty<int>()
            })
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _ollamaProvider.CompleteAsync("Test prompt", null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.Response);
    }

    private void SetupProvider()
    {
        var options = new OllamaOptions()
        {
            Model = "test-model",
            ServerUrl = "http://test-server"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _ollamaProvider.InitProvider(options);
    }

    private IConversationContext CreateMockContext()
    {
        var mockContext = Substitute.For<IConversationContext>();
        mockContext.GetConverstationHistory().Returns(new List<ConversationItem>());
        return mockContext;
    }
}
