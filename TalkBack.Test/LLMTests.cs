using Microsoft.Extensions.Logging;
using NSubstitute;
using TalkBack.Exceptions;
using TalkBack.Interfaces;
using TalkBack.LLMProviders.Claude;

namespace TalkBack.Test
{
    public class LLMTests
    {
        private readonly ILogger<LLM> _logger = Substitute.For<ILogger<LLM>>();
        private readonly ILLMProvider _provider = Substitute.For<ILLMProvider>();
        private readonly LLM _llm;

        public LLMTests()
        {
            _llm = new LLM(_logger);
        }

        [Fact]
        public void CreateNewContext_WithoutProvider_ThrowsNoProviderSetException()
        {
            // Act & Assert
            Assert.Throws<NoProviderSetException>(() => _llm.CreateNewContext());
        }

        [Fact]
        public void CreateNewContext_WithProvider_ReturnsContext()
        {
            // Arrange
            _llm.SetProvider(_provider);
            var expectedContext = Substitute.For<IConversationContext>();

            _provider.CreateNewContext().Returns(expectedContext);

            // Act
            var result = _llm.CreateNewContext();

            // Assert
            Assert.Equal(expectedContext, result);
        }

        [Fact]
        public async Task StreamCompletionAsync_WithoutProvider_ThrowsNoProviderSetException()
        {
            // Arrange
            var receiver = Substitute.For<ICompletionReceiver>();
            var prompt = "Test Prompt";


            // Act & Assert
            await Assert.ThrowsAsync<NoProviderSetException>(() => _llm.StreamCompletionAsync(receiver, prompt));
        }

        [Fact]
        public async Task StreamCompletionAsync_WithProvider_CallsProviderMethod()
        {
            // Arrange
            _llm.SetProvider(_provider);
            var receiver = Substitute.For<ICompletionReceiver>();
            var prompt = "Test Prompt";
            var context = Substitute.For<IConversationContext>();

            // Act
            await _llm.StreamCompletionAsync(receiver, prompt, context);

            // Assert
            await _provider.Received().StreamCompletionAsync(receiver, prompt, context);
        }

        [Fact]
        public async Task CompleteAsync_WithoutProvider_ThrowsNoProviderSetException()
        {
            // Arrange
            var prompt = "Test Prompt";

            // Act & Assert
            await Assert.ThrowsAsync<NoProviderSetException>(() => _llm.CompleteAsync(prompt));
        }

        [Fact]
        public async Task CompleteAsync_WithProvider_CallsProviderMethodAndReturnsResult()
        {
            // Arrange
            _llm.SetProvider(_provider);
            var prompt = "Test Prompt";
            var context = Substitute.For<IConversationContext>();
            var modelResponse = Substitute.For<IModelResponse>();

            _provider.CompleteAsync(prompt, context).Returns(Task.FromResult(modelResponse));

            // Act
            var result = await _llm.CompleteAsync(prompt, context);

            // Assert
            Assert.Equal(modelResponse, result);
        }
    }
}
