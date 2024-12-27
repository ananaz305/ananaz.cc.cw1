using System.Text.Json.Serialization;

namespace Piazza.API.Models;

public class CreateComment
{
    [JsonPropertyName("content")]
    public required string Content { get; set; }

    [JsonPropertyName("blogId")]
    public required string BlogId { get; set; }

    [JsonPropertyName("replyTo")]
    public string? ReplyTo { get; set; }
}