using Microsoft.EntityFrameworkCore;
using Piazza.Database.Models;

namespace Piazza.Database;

public sealed class PiazzaContext : DbContext
{
    public PiazzaContext(DbContextOptions<PiazzaContext> options) : base(options)
    {
        Database.EnsureCreated();
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Blog> Blogs { get; set; }
    public DbSet<Comment> Comments { get; set; }
}