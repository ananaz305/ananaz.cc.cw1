using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Diagnostics.CodeAnalysis;

namespace Piazza.Database.Models;

[Table("comments")]
public class Comment : IEntity
{
    public Guid Id { get; set; }

    [Column("user_id"), Required]
    public required User User { get; set; }


    [Column("blog_id"), Required]
    public required Blog Blog { get; set; }

    [Column("reply_to")]
    public Guid ReplyToCommentId { get; set; }

    [Column("content"), Required]
    public required string Content { get; set; }
}