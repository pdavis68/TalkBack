
using TalkBackChatServer.Models;

namespace TalkBackChatServer.DTOs
{
    public class ChatMessageDto
    {
        public int ConversationId { get; set; }
        public string? Message { get; set; }
    }
}