using TalkBack.Interfaces;
using TalkBack.LLMProviders.Claude;
using TalkBack.LLMProviders.Groq;
using TalkBack.LLMProviders.Ollama;
using TalkBack.LLMProviders.OpenAI;
using TalkBack.Models;
using TalkBackChatServer.Data;
using TalkBackChatServer.Models;

namespace TalkBackChatServer.Services
{
    public interface IChatService
    {
        List<string> GetProviders();
        Task<List<ILLMModel>> GetModelsAsync(string providerName);
        Task<string> ChatAsync(string message, int conversationId, Action<string>? callback = null);
        Task<string> GenerateTitleAsync(string text);
    }

    public class ChatService : IChatService, ICompletionReceiver
    {

        private readonly ILogger _logger;
        private readonly ApplicationDbContext _dbContext;
        private readonly ILLM _llm;
        private readonly ILLMConfig _llmConfig;
        private readonly IProviderActivator _providerActivator;
        private ILLMProvider? _llmProvider;

        // Kinda sketchy, but fine for a demo.
        // Maybe when I'm not so lazy, I'll have ChatGPT fix it.
        private Action<string>? _callback;
        private int _conversationId;

        public ChatService(ILogger<ChatService> logger,
            ApplicationDbContext dbContext,   
            ILLM llm,
            IProviderActivator providerActivator,
            ILLMConfig llmConfig)
        {
            _logger = logger;
            _dbContext = dbContext;
            _llm = llm;
            _providerActivator = providerActivator;
            _llmConfig = llmConfig;
        }

        public List<string> GetProviders()
        {
            _logger.LogInformation("Getting providers");
            return new List<string> { Constants.OpenAI, Constants.Claude, Constants.Groq, Constants.Ollama };
        }

        public async Task<List<ILLMModel>> GetModelsAsync(string providerName)
        {
            if (!GetProviders().Contains(providerName))
            {
                throw new InvalidOperationException($"Invalid providerName {providerName}");
            }
            var llmProvider = GetProvider(providerName);
            return await llmProvider.GetModelsAsync();
        }


        public async Task<string> ChatAsync(string message, int conversationId, Action<string>? callback = null)
        {
            _logger.LogInformation($"ChatAsync - {conversationId}");
            var dbConversation = _dbContext.Conversations.FirstOrDefault(p=>p.Id == conversationId);
            if (dbConversation is null)
            {
                throw new Exception($"Conversation with id {conversationId} not found");
            }
            _conversationId = conversationId;
            var dbMessages = _dbContext.Messages.Where(p=>p.ConversationId == conversationId).ToList();
            if (dbConversation.Title == Constants.Pending)
            {
                dbConversation.Title = await GenerateTitleAsync(message);
            }

            var conversationHistory = new List<ConversationItem>();
            for (int index = 0; index < dbMessages.Count; index += 2)
            {                
                conversationHistory.Add(new ConversationItem()
                {
                    User = dbMessages[index].Content ?? "",
                    Assistant = dbMessages[index + 1].Content ?? ""
                });
            }

            // Add user message to db
            _dbContext.Messages.Add(new Message() { ConversationId = _conversationId, Content = message, Role = Constants.User });
            await _dbContext.SaveChangesAsync();

            _llmProvider = GetProvider(dbConversation.LLM!, dbConversation.Model);
            _llm.SetProvider(_llmProvider);
            var context = _llmProvider.CreateNewContext(dbConversation.SystemMessage, conversationHistory);
            _callback = callback;
            if (callback is null)
            {
                var response = await _llm.CompleteAsync(message, context);
                // Add assistant message to db
                _logger.LogInformation($"Inserting Assistant message {response.Response}");
                dbMessages.Add(new Message() { ConversationId = _conversationId, Content = response.Response, Role = Constants.Assistant });
                await _dbContext.SaveChangesAsync();
                return response.Response ?? "";
            }
            else
            {
                await _llm.StreamCompletionAsync(this, message, context);
                return "";
            }
        }

        public async Task<string> GenerateTitleAsync(string text)
        {
            var origProvider = _llm.Provider;

            _llmProvider = GetProvider(_llmConfig.TitleLLM??"Something New", _llmConfig.TitleModel);
            _llm.SetProvider(_llmProvider);
            var prompt = $"Below is the text that a user typed. It's probably a question, but maybe not." +
                $" Please generate a short title for what they typed. Try to keep the title under about 50 characters. Respond ONLY with the title!" +
                $" Be as accurate, factual and concise as possible. Avoid humor, opinion, or speculation." +
                $" For example, if the user typed 'What is the capital of France?', you might respond with: 'Capital of France'. Don't offer options." +
                $" Don't ask my opinion. Just give me a title." +
                $" The user typed: {text}";

            var result = await _llm.CompleteAsync(prompt);

            // Reset to original provider
            if (origProvider is not null)
            {
                _llm.SetProvider(origProvider);
            }

            return result.Response?? "Error";
        }

        public ILLMProvider GetProvider(string providerName, string? modelName = null)
        {
            ILLMProvider? provider;
            switch(providerName)
            {
                case Constants.Groq:
                    provider = _providerActivator.CreateProvider<GroqProvider>()!;
                    provider.InitProvider(GetGroqOptions(modelName));
                    break;
                case Constants.OpenAI:
                    provider = _providerActivator.CreateProvider<OpenAIProvider>()!;
                    provider.InitProvider(GetOpenAIOptions(modelName));
                    break;
                case Constants.Claude:
                    provider = _providerActivator.CreateProvider<ClaudeProvider>()!;
                    provider.InitProvider(GetClaudeOptions(modelName));
                    break;
                case Constants.Ollama:
                    provider = _providerActivator.CreateProvider<OllamaProvider>()!;
                    provider.InitProvider(GetOllamaOptions(modelName));
                    break;
                default:
                    throw new InvalidOperationException($"Invalid providerName {providerName}");
            }
            return provider!;
        }

        private GroqOptions GetGroqOptions(string? modelName)
        {
            return new GroqOptions()
            {
                ApiKey = _llmConfig.GroqKey,
                Model = string.IsNullOrWhiteSpace(modelName) ? "gemma2-9b-it" : modelName,
                Temperature = 1,
                MaxTokens = 2048,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                TopP = 1,
                Stop = "",
            };
        }

        private OpenAIOptions GetOpenAIOptions(string? modelName)
        {
            return new OpenAIOptions()
            {
                ApiKey = _llmConfig.OpenAIKey,
                Model = string.IsNullOrWhiteSpace(modelName) ? "gpt-4o" : modelName,
                Temperature = 1,
                MaxTokens = 2048,
                FrequencyPenalty = 0,
                PresencePenalty = 0,
                TopP = 1,
                Stop = "",
            };
        }

        private ClaudeOptions GetClaudeOptions(string? modelName)
        {
            return new ClaudeOptions()
            {
                ApiKey = _llmConfig.ClaudeKey,
                Model = string.IsNullOrWhiteSpace(modelName) ? "claude-3-5-sonnet-20240620" : modelName,
                Temperature = 1,
                TopP = 1,
                TopK = 50,
                StopSequences =  new string[] { "\n\nHuman:" },
                AnthropicVersion = "2023-06-01",
                MaxTokensToSample =  256
            };
        }

        private OllamaOptions GetOllamaOptions(string? modelName)
        {
            return new OllamaOptions()
            {
                ServerUrl = _llmConfig.OllamaUrl,
                Model = string.IsNullOrWhiteSpace(modelName) ? _llmConfig.DefaultOllamaModel : modelName,
                Temperature = 1,
                TopP =  1,
                TopK =  50,
                NumPredict =  1,
                RepeatPenalty =  0,
                FrequencyPenalty =  0,
                PresencePenalty =  0,
                Seed = 0
            };
        }

        public async Task ReceiveCompletionPartAsync(IModelResponse response, bool final)
        {
            await Task.Run(() =>
            {
                if (!final)
                {
                    _callback!(response.Response ?? "");
                }
                else
                {
                    // The "final" message has the complete response.
                    // Add assistant message to db
                    _logger.LogInformation($"Inserting Assistant message {response.Response}");
                    _dbContext.Messages.Add(new Message() { ConversationId = _conversationId, Content = response.Response, Role = Constants.Assistant });
                    _dbContext.SaveChangesAsync();
                    _callback!(Constants.Done);
                    _callback = null;
                }
            });
        }
    }
}