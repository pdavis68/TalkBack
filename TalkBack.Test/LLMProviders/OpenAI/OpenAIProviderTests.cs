using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.OpenAI;
using TalkBack.Models;

namespace TalkBack.Test.LLMProviders.OpenAI;

public class OpenAIProviderTests
{
    private readonly ILLMProvider _openAIProvider;
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<OpenAIProvider> _logger = Substitute.For<ILogger<OpenAIProvider>>();

    public OpenAIProviderTests()
    {
        _httpHandler = Substitute.For<IHttpHandler>();
        _openAIProvider = new OpenAIProvider(_logger, _httpHandler);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _openAIProvider.CompleteAsync("test prompt", null));
    }

    [Fact]
    public async Task CompleteAsync_Success_WhenInitProviderCalled()
    {
        // Arrange
        var options = new OpenAIOptions()
        {
            Model = "test-model",
            ApiKey = "test-api-key"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _openAIProvider.InitProvider(options);

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new OpenAICompletionsResponse
            {
                Choices = new[]
                {
                    new OpenAIChoice
                    {
                        Message = new OpenAIReceivedMessage { Content = "Hello, this is a test response." }
                    }
                }
            })
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var context = _openAIProvider.CreateNewContext();
        var response = await _openAIProvider.CompleteAsync("Test prompt", context);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Hello, this is a test response.", response.Response);
        Assert.NotNull(response.Context);

        var conversationHistory = response.Context.GetConverstationHistory().ToList();
        Assert.Single(conversationHistory);
        Assert.Equal("Test prompt", conversationHistory[0].User);
        Assert.Equal("Hello, this is a test response.", conversationHistory[0].Assistant);
    }

    [Fact]
    public async Task CompleteAsync_HandlesEmptyResponse()
    {
        // Arrange
        SetupProvider();

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new { choices = Array.Empty<object>() })
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _openAIProvider.CompleteAsync("Test prompt", null));
    }

    [Fact]
    public async Task CompleteAsync_HandlesNullContent()
    {
        // Arrange
        SetupProvider();

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new { content = (string?)null }
                    }
                }
            })
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _openAIProvider.CompleteAsync("Test prompt", null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal(string.Empty, response.Response);
    }

    private void SetupProvider()
    {
        var options = new OpenAIOptions()
        {
            Model = "test-model",
            ApiKey = "test-api-key"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _openAIProvider.InitProvider(options);
    }

    private IConversationContext CreateMockContext()
    {
        var mockContext = Substitute.For<IConversationContext>();
        mockContext.GetConverstationHistory().Returns(new List<ConversationItem>());
        return mockContext;
    }
}
