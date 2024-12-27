using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Piazza.API.Models;
using Piazza.API.Services;
using Piazza.Database;
using Piazza.Database.Models;

namespace Piazza.API.Controllers;

[Authorize]
[ApiController]
[Route("api/blog")]
public class BlogController(PiazzaContext piazzaContext, JwtService jwtService) : ControllerBase
{
    [HttpPost("create")]
    public async Task<ActionResult> CreateBlogPost([FromBody] CreateBlog createBlog)
    {
        var header = this.HttpContext.Request.Headers["Authorization"].FirstOrDefault()!.Split(' ')[1];

        var id = jwtService.GetIdFromToken(header);
        var user = await piazzaContext.Users.FindAsync(Guid.Parse(id));

        if (user == null)
        {
            return BadRequest();
        }

        var entity = new Blog()
        {
            Id = Guid.NewGuid(),
            Title = createBlog.Title,
            Body = createBlog.Body,
            User = user,
            CreatedAt = DateTime.UtcNow,
            BlogType = Blog.ParseFromStringBlogType(createBlog.BlogType),
        };

        await piazzaContext.Blogs.AddAsync(entity);
        await piazzaContext.SaveChangesAsync();

        return Ok(new { id = entity.Id });
    }

    [HttpGet("get/{id}")]
    public async Task<ActionResult> GetByIdBlogPost(string id)
    {
        var blog = await piazzaContext.Blogs
            .Include(b => b.User).Include(blog => blog.Likes).Include(blog => blog.Dislikes)
            .FirstOrDefaultAsync(x => x.Id.ToString() == id);

        if (blog == null)
        {
            return NotFound();
        }

        var comments = await piazzaContext.Comments.Include(x => x.Blog).Where(x => x.Blog.Id == blog.Id).Select(x => x.Id.ToString()).ToListAsync();

        return Ok(new
        {
            title = blog.Title,
            body = blog.Body,
            creator = blog.User.Id,
            createdAt = blog.CreatedAt,
            blogType = blog.BlogType.ToString(),
            isExpired = blog.CreatedAt.AddDays(1) < DateTime.UtcNow,
            likes = blog.Likes.Count,
            dislikes = blog.Dislikes.Count,
            comments
        });
    }

    [HttpGet("get-all")]
    public async Task<ActionResult> GetAllBlogPosts()
    {
        var blogs = (await piazzaContext.Blogs.ToListAsync()).Select(x => x.Id.ToString());

        return Ok(blogs);
    }

    [HttpPost("comment")]
    public async Task<ActionResult> CreateComment([FromBody] CreateComment createComment)
    {
        var header = this.HttpContext.Request.Headers["Authorization"].FirstOrDefault()!.Split(' ')[1];
        var id = jwtService.GetIdFromToken(header);
        var user = await piazzaContext.Users.FindAsync(Guid.Parse(id));

        if (user == null)
        {
            return BadRequest();
        }

        var blog = await piazzaContext.Blogs.FindAsync(Guid.Parse(createComment.BlogId));

        if (blog == null)
        {
            return BadRequest();
        }

        var comment = new Comment()
        {
            Id = Guid.NewGuid(),
            Content = createComment.Content,
            User = user,
            Blog = blog,
        };

        if (createComment.ReplyTo != null)
        {
            comment.ReplyToCommentId = Guid.Parse(createComment.ReplyTo);
        }

        await piazzaContext.Comments.AddAsync(comment);
        await piazzaContext.SaveChangesAsync();

        return Ok(new { id = comment.Id });
    }

    [HttpGet("get-comment/{id}")]
    public async Task<ActionResult> GetCommentById(string id)
    {
        var comment = await piazzaContext.Comments.Include(x => x.User).Include(x => x.Blog)
            .FirstOrDefaultAsync(x => x.Id.ToString() == id);

        if (comment == null)
        {
            return NotFound();
        }

        return Ok(new
        {
            content = comment.Content,
            creator = comment.User.Id,
            blog = comment.Blog.Id,
            replyTo = comment.ReplyToCommentId
        });
    }

    [HttpGet("get-comments/{id}")]
    public Task<ActionResult> GetComments(string id)
    {
        var commentToBlog = piazzaContext.Comments.Include(x => x.Blog).Where(x => x.Blog.Id.ToString() == id);

        return Task.FromResult<ActionResult>(Ok(commentToBlog));
    }

    [HttpPost("like/{id}")]
    public async Task<ActionResult> LikeBlogPost(string id)
    {
        var header = this.HttpContext.Request.Headers["Authorization"].FirstOrDefault()!.Split(' ')[1];
        var userId = jwtService.GetIdFromToken(header);

        var user = await piazzaContext.Users.FindAsync(Guid.Parse(userId));

        if (user == null)
        {
            return BadRequest();
        }

        var blog = await piazzaContext.Blogs.FindAsync(Guid.Parse(id));

        if (blog == null)
        {
            return BadRequest();
        }

        blog.Likes.Add(user);

        piazzaContext.Update(blog);
        await piazzaContext.SaveChangesAsync();

        return Ok(new { id = blog.Id });
    }

    [HttpPost("dislike/{id}")]
    public async Task<ActionResult> DislikeBlogPost(string id)
    {
        var header = this.HttpContext.Request.Headers["Authorization"].FirstOrDefault()!.Split(' ')[1];
        var userId = jwtService.GetIdFromToken(header);

        var user = await piazzaContext.Users.FindAsync(Guid.Parse(userId));

        if (user == null)
        {
            return BadRequest();
        }

        var blog = await piazzaContext.Blogs.FindAsync(Guid.Parse(id));

        if (blog == null)
        {
            return BadRequest();
        }

        blog.Dislikes.Add(user);

        piazzaContext.Update(blog);
        await piazzaContext.SaveChangesAsync();

        return Ok(new { id = blog.Id });
    }
}