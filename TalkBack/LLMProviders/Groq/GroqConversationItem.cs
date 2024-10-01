using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.Groq;

public class GroqConversationItem
{
    public GroqConversationItem(string role, string content)
    {
        Role = role;
        Content = content;
    }
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
