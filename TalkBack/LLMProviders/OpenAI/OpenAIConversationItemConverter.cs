namespace TalkBack.LLMProviders.OpenAI;

using System.Text.Json;
using System.Text.Json.Serialization;

public class OpenAIConversationItemConverter : JsonConverter<OpenAIConversationItem>
{
    public override OpenAIConversationItem Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        // Not needed. We only need to serialize ConversationItem objects.
        throw new NotImplementedException();
    }

    public override void Write(Utf8JsonWriter writer, OpenAIConversationItem value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        writer.WriteString("role", value.Role);

        if (value.Content.Count == 1 && value.Content[0].Type == "text")
        {
            writer.WriteString("content", value.Content[0].Text);
        }
        else
        {
            writer.WritePropertyName("content");
            JsonSerializer.Serialize(writer, value.Content, options);
        }

        writer.WriteEndObject();
    }
}
