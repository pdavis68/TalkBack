using System.ComponentModel.DataAnnotations;

namespace TalkBackChatServer.Models
{
    public class Conversation
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? LLM { get; set; }

        [Required]
        public string? Model { get; set; }

        [Required]
        public string SystemMessage { get; set; } = "You are a helpful assistant";

        public ICollection<Message>? Messages { get; set; }
    }
}
