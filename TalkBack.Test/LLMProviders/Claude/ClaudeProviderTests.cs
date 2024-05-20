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
    private readonly ILogger<ClaudeProvider>  _logger = Substitute.For<ILogger<ClaudeProvider>>();

    public ClaudeProviderTests()
    {

        _httpHandler = Substitute.For<IHttpHandler>();
        _claudeProvider = new ClaudeProvider(_logger, _httpHandler);
    }

    [Fact]
    public async Task CompleteAsync_ThrowsException_WhenInitProviderNotCalled()
    {
        // Arrange 

        // oops

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _claudeProvider.CompleteAsync("test prompt"));

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

        var headers = new HttpRequestMessage().Headers;
        _httpHandler.DefaultRequestHeaders.Returns(headers);
        _claudeProvider.InitProvider(options);


        var mi = new ClaudeMessageItem[1];
        mi[0] = new ClaudeMessageItem()
        {
            Text = "hello"
        };
        ClaudeMessageResponse cmr = new ClaudeMessageResponse()
        {
            Content = mi
        };
        var successfulResponse = new HttpResponseMessage(HttpStatusCode.OK)
        {

            Content = JsonContent.Create(cmr)
        };


        // Act & Assert
        _httpHandler.PostAsync(Arg.Any<string>(), Arg.Any<HttpContent>())
            .Returns(Task.FromResult(successfulResponse));
        var response = await _claudeProvider.CompleteAsync("test prompt");

        // Additional assertions based on expected behavior of CompleteAsync
        // Note: This example assumes a successful completion, adjust according to your expectations
        Assert.NotNull(response);
    }
}
