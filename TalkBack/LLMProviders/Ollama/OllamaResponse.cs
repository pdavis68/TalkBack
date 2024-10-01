using TalkBack.Interfaces;

namespace TalkBack.LLMProviders.Ollama;

public class OllamaResponse : IModelResponse
{
    public IConversationContext? Context { get; set; }

    public string Response { get; set; } = string.Empty;
}
