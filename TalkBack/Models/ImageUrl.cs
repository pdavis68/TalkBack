using System.Text.Json.Serialization;

namespace TalkBack.Models;

public class ImageUrl
{
    [JsonPropertyName("url")]
    public string? Url { get; set; } = string.Empty;
    [JsonPropertyName("detail")]
    public string? Detail { get; set; } = string.Empty;
}
