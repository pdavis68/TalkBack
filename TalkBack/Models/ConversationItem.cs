namespace TalkBack.Models;

public class ConversationItem
{
    public string User { get; set; }  = string.Empty;
    public string Assistant { get; set; } = string.Empty;
    public List<ImageUrl>? ImageUrls { get; set; } = null;
}
