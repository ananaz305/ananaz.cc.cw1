using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using Microsoft.EntityFrameworkCore;

namespace Piazza.Database.Models;

public interface IEntity
{
    [Column("id"), Key]
    Guid Id { get; set; }
}