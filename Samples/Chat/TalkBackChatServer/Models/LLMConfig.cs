namespace TalkBackChatServer.Models
{
    public interface ILLMConfig
    {
        string? GroqKey { get; }
        string? OpenAIKey { get; }
        string? ClaudeKey { get; }
        string? OllamaUrl { get; }
        string? DefaultOllamaModel { get; }
        string? TitleLLM { get; }
        string? TitleModel { get; }
    }

    public class LLMConfig : ILLMConfig
    {
        public string? GroqKey { get; set; }
        public string? OpenAIKey { get; set; }
        public string? ClaudeKey { get; set; }
        public string? OllamaUrl { get; set; }
        public string? DefaultOllamaModel { get; set; }
        public string? TitleLLM { get; set; }
        public string? TitleModel { get; set; }
    }
}
