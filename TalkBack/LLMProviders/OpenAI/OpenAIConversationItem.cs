using System.Text.Json.Serialization;
using TalkBack.Models;

namespace TalkBack.LLMProviders.OpenAI;

internal class OpenAIConversationItem
{
    public OpenAIConversationItem(string role, List<ContentItem> content)
    {
        Role = role;
        Content = content;
    }

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public List<ContentItem> Content { get; set; }
}

internal class OpenAIReceivedMessage
{
    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public string? Content { get; set; }

}

internal class ContentItem
{
    [JsonPropertyName("type")]
    public string? Type { get; set; }

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("image_url")]
    public ImageUrl? ImageUrl { get; set; }
}

