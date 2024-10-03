using System.ComponentModel.DataAnnotations;

namespace TalkBackChatServer.Models
{
    public class Message
    {
        public int Id { get; set; }

        [Required]
        public int ConversationId { get; set; }

        public Conversation? Conversation { get; set; }

        [Required]
        public int Index { get; set; }

        [Required]
        public string? Role { get; set; }

        [Required]
        public string? Content { get; set; }
    }
}
