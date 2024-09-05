namespace TalkBack.LLMProviders.OpenAI;

public class OpenAIModelList
{
    public string Object { get; set; } = string.Empty;
    public OpenAIModel[] Data { get; set; } = Array.Empty<OpenAIModel>();
}
