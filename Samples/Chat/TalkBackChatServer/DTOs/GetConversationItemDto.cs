using System.ComponentModel.DataAnnotations;

namespace TalkBackChatServer.DTOs
{
    public class GetConversationItemDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
    }
}
