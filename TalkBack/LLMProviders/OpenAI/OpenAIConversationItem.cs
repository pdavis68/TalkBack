
using System.Text.Json.Serialization;

namespace TalkBack.LLMProviders.OpenAI;

internal class OpenAIConversationItem
{
    public OpenAIConversationItem(string role, string content)
    {
        Role = role;
        Content = content;
    }
    [JsonPropertyName("role")]
    public string? Role { get; set; }
    [JsonPropertyName("content")]
    public string? Content { get; set; }
}
