using TalkBack.LLMProviders.OpenAI;

namespace TalkBack.LLMProviders.Groq;

public class GroqModelList
{
    public string Object { get; set; } = string.Empty;
    public GroqModel[] Data { get; set; } = Array.Empty<GroqModel>();
}
