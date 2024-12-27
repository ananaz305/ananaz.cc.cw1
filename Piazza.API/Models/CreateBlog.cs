using System.Text.Json.Serialization;

namespace Piazza.API.Models;

public class CreateBlog
{
    [JsonPropertyName("title")]
    public required string Title { get; set; }
    [JsonPropertyName("body")]
    public required string Body { get; set; }

    [JsonPropertyName("blogType")]
    public required string BlogType { get; set; }
}