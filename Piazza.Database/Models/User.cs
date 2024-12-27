using System.ComponentModel.DataAnnotations.Schema;

namespace Piazza.Database.Models;

[Table("users")]
public class User : IEntity
{
    public Guid Id { get; set; }

    [Column("username")]
    public required string Username { get; set; }
}