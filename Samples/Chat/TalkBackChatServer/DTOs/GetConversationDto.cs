using System.Text.Json.Serialization;

namespace TalkBackChatServer.DTOs
{
    public class GetConversationDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? LLM { get; set; }
        public string? Model { get; set; }

        [JsonPropertyName("system-message")]
        public string? SystemMessage { get; set; }

        public List<GetMessageDto>? Messages { get; set; }
    }
}
