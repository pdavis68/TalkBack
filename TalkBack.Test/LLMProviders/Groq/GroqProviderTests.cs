using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Groq;

namespace TalkBack.Test.LLMProviders.Groq;

public class GroqProviderTests
{
    private readonly GroqProvider _groqProvider;
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<GroqProvider> _logger = Substitute.For<ILogger<GroqProvider>>();

    public GroqProviderTests()
    {
        _httpHandler = Substitute.For<IHttpHandler>();
        _groqProvider = new GroqProvider(_logger, _httpHandler);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange & Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _groqProvider.CompleteAsync("test prompt"));
    }

    [Fact]
    public async Task CompleteAsync_Success_WhenInitProviderCalled()
    {
        // Arrange
        var options = new GroqOptions()
        {
            Model = "test-model",
            ApiKey = "test-api-key"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _groqProvider.InitProvider(options);

        var choice = new GroqChoice()
        {
            Message = new GroqConversationItem("assistant", "Hello, this is a test response.")
        };
        var completionResponse = new GroqCompletionsResponse()
        {
            Choices = new[] { choice }
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(completionResponse)
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _groqProvider.CompleteAsync("Test prompt");

        // Assert
        Assert.NotNull(response);
        Assert.IsType<GroqResponse>(response);
        Assert.Equal("Hello, this is a test response.", response.Response);
        Assert.NotNull(response.Context);
        Assert.IsType<GroqContext>(response.Context);

        var context = response.Context as GroqContext;
        Assert.NotNull(context);
        Assert.Single(context.Conversation);
        Assert.Equal("Test prompt", context.Conversation[0].User);
        Assert.Equal("Hello, this is a test response.", context.Conversation[0].Assistant);
    }

    [Fact]
    public async Task CompleteAsync_HandlesEmptyResponse()
    {
        // Arrange
        SetupProvider();

        var completionResponse = new GroqCompletionsResponse()
        {
            Choices = Array.Empty<GroqChoice>()
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(completionResponse)
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _groqProvider.CompleteAsync("Test prompt"));
    }

    [Fact]
    public async Task CompleteAsync_HandlesNullContent()
    {
        // Arrange
        SetupProvider();

        var completionResponse = new GroqCompletionsResponse()
        {
            Choices = new[] { new GroqChoice { Message = null } }
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(completionResponse)
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _groqProvider.CompleteAsync("Test prompt");

        // Assert
        Assert.NotNull(response);
        Assert.IsType<GroqResponse>(response);
        Assert.Equal(string.Empty, response.Response);
    }

    private void SetupProvider()
    {
        var options = new GroqOptions()
        {
            Model = "test-model",
            ApiKey = "test-api-key"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _groqProvider.InitProvider(options);
    }
}
