using Microsoft.Extensions.Logging;
using NSubstitute;
using System.Net;
using System.Net.Http.Json;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Groq;

namespace TalkBack.Test.LLMProviders.Groq;

public class GroqProviderTests
{
    private readonly ILLMProvider _groqProvider;
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
        await Assert.ThrowsAsync<InvalidOperationException>(() => _groqProvider.CompleteAsync("test prompt", null));
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

        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {
            Content = JsonContent.Create(new
            {
                choices = new[]
                {
                    new
                    {
                        message = new
                        {
                            content = "Hello, this is a test response."
                        }
                    }
                }
            })
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _groqProvider.CompleteAsync("Test prompt", null);

        // Assert
        Assert.NotNull(response);
        Assert.Equal("Hello, this is a test response.", response.Response);
        Assert.NotNull(response.Context);
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
        await Assert.ThrowsAsync<InvalidOperationException>(() => _groqProvider.CompleteAsync("Test prompt", null));
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
                        message = new { content = (string)null }
                    }
                }
            })
        };

        _httpHandler.SendAsync(Arg.Any<HttpRequestMessage>(), Arg.Any<HttpCompletionOption>())
            .Returns(Task.FromResult(successfulResponse));

        // Act
        var response = await _groqProvider.CompleteAsync("Test prompt", null);

        // Assert
        Assert.NotNull(response);
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
