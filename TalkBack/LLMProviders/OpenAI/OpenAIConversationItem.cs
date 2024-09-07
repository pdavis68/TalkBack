using System.Text.Json;
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
    [JsonConverter(typeof(ImageUrlConditionalConverter))]
    public ImageUrl? ImageUrl { get; set; }
}

public class ImageUrlConditionalConverter : JsonConverter<ImageUrl?>
{
    public override ImageUrl? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Implementation for reading is not needed in this case
        return null;
    }

    public override void Write(Utf8JsonWriter writer, ImageUrl? value, JsonSerializerOptions options)
    {
        if (value != null && !string.IsNullOrEmpty(value.Url))
        {
            writer.WritePropertyName("image_url");
            JsonSerializer.Serialize(writer, value);
        }
    }
}