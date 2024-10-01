using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Claude;

namespace TalkBack.Test.LLMProviders.Claude;

public class ClaudeProviderTests
{
    private readonly ClaudeProvider _claudeProvider;
    private readonly IHttpHandler _httpHandler;
    private readonly ILogger<ClaudeProvider> _logger = Substitute.For<ILogger<ClaudeProvider>>();

    public ClaudeProviderTests()
    {
        _httpHandler = Substitute.For<IHttpHandler>();
        _claudeProvider = new ClaudeProvider(_logger, _httpHandler);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange 
        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claudeProvider.CompleteAsync("test prompt"));
    }

    [Fact]
    public async Task CompleteAsync_Success_WhenInitProviderCalled()
    {
        // Arrange
        SetupProvider();

        var messageItem = new ClaudeMessageItem()
        {
            Text = "Hello, this is a test response.",
            Type = "text"
        };
        var messageResponse = new ClaudeMessageResponse()
        {
            Content = new[] { messageItem },
            StopReason = "end_turn",
            Model = "test-model",
            Role = "assistant"
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(messageResponse)
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var context = _claudeProvider.CreateNewContext();
        var response = await _claudeProvider.CompleteAsync("Test prompt", context);

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

        var messageResponse = new ClaudeMessageResponse()
        {
            Content = Array.Empty<ClaudeMessageItem>(),
            StopReason = "end_turn",
            Model = "test-model",
            Role = "assistant"
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(messageResponse)
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _claudeProvider.CompleteAsync("Test prompt");

        // Assert
        Assert.NotNull(response);
        Assert.IsType<ClaudeResponse>(response);
        Assert.Equal(string.Empty, response.Response);
    }

    [Fact]
    public async Task CompleteAsync_HandlesNullContent()
    {
        // Arrange
        SetupProvider();

        var messageResponse = new ClaudeMessageResponse()
        {
            Content = null,
            StopReason = "end_turn",
            Model = "test-model",
            Role = "assistant"
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(messageResponse)
        };

        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _claudeProvider.CompleteAsync("Test prompt");

        // Assert
        Assert.NotNull(response);
        Assert.IsType<ClaudeResponse>(response);
        Assert.Equal(string.Empty, response.Response);
    }

    private void SetupProvider()
    {
        var options = new ClaudeOptions()
        {
            Model = "test-model",
            AnthropicVersion = "2023-06-01",
            ApiKey = "test-api-key"
        };

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _claudeProvider.InitProvider(options);
    }

    private IConversationContext CreateMockContext()
    {
        var mockContext = Substitute.For<IConversationContext>();
        mockContext.GetConverstationHistory().Returns(new List<ConversationItem>());
        return mockContext;
    }
}
