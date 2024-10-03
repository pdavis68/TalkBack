namespace TalkBackChatServer.DTOs
{    public class GetMessageDto
    {
        public int Index { get; set; }
        public string? Role { get; set; }
        public string? Content { get; set; }
    }
}