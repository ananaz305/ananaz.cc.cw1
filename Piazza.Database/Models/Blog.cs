using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Piazza.Database.Models;

[Table("blogs")]
public class Blog : IEntity
{
    public Guid Id { get; set; }

    [Column("user_id"), Required]
    public required User User { get; set; }

    [Column("blog_type"), Required]
    public required BlogType BlogType { get; set; }

    [Column("title"), Required]
    public required string Title { get; set; }

    [Column("body"), Required]
    public required string Body { get; set; }

    [Column("created_at"), Required]
    public DateTime CreatedAt { get; set; }

    [Column("likes")]
    public List<User> Likes { get; set; } = new();

    [Column("dislikes")]
    public List<User> Dislikes { get; set; } = new();

    public static BlogType ParseFromStringBlogType(string blogType)
    {
        return blogType switch
        {
            "political" => BlogType.Political,
            "sport" => BlogType.Sport,
            "health" => BlogType.Health,
            _ => BlogType.Technology
        };
    }
}

public enum BlogType
{
    Political,
    Sport,
    Health,
    Technology,
}