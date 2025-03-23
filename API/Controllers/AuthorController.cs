using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;

namespace Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthorController(AppDbContext context, ILogger<AuthorController> logger) : ControllerBase
{
    private readonly ILogger<AuthorController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
    {
        var authors = await context.Authors
            .Select(a => new AuthorDto
            {
                AuthorId = a.AuthorId,
                Name = a.Name,
                Biography = a.Biography
            })
            .ToListAsync();

        return Ok(authors);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDetailDto>> GetAuthor(int id)
    {
        var author = await context.Authors
            .Include(a => a.Books)
            .FirstOrDefaultAsync(a => a.AuthorId == id);

        if (author == null)
        {
            return NotFound();
        }

        var authorDetail = new AuthorDetailDto
        {
            AuthorId = author.AuthorId,
            Name = author.Name,
            Biography = author.Biography,
            Books = author.Books.Select(b => new BookBriefDto
            {
                BookId = b.BookId,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl,
                PublicationYear = b.PublicationYear
            }).ToList()
        };

        return Ok(authorDetail);
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
    {
        var author = new Author
        {
            Name = createAuthorDto.Name,
            Biography = createAuthorDto.Biography,
            Books = new List<Book>()
        };

        context.Authors.Add(author);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetAuthor), new { id = author.AuthorId }, new AuthorDto
        {
            AuthorId = author.AuthorId,
            Name = author.Name,
            Biography = author.Biography
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuthor(int id, UpdateAuthorDto updateAuthorDto)
    {
        var author = await context.Authors.FindAsync(id);
        if (author == null)
        {
            return NotFound();
        }

        author.Name = updateAuthorDto.Name ?? author.Name;
        author.Biography = updateAuthorDto.Biography ?? author.Biography;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AuthorExists(id))
            {
                return NotFound();
            }
            else
            {
                throw;
            }
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        var author = await context.Authors.FindAsync(id);
        if (author == null)
        {
            return NotFound();
        }

        context.Authors.Remove(author);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("{id}/books")]
    public async Task<ActionResult<IEnumerable<BookBriefDto>>> GetAuthorBooks(int id)
    {
        if (!AuthorExists(id))
        {
            return NotFound();
        }

        var books = await context.Authors
            .Where(a => a.AuthorId == id)
            .SelectMany(a => a.Books)
            .Select(b => new BookBriefDto
            {
                BookId = b.BookId,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl,
                PublicationYear = b.PublicationYear
            })
            .ToListAsync();

        return Ok(books);
    }

    private bool AuthorExists(int id)
    {
        return context.Authors.Any(e => e.AuthorId == id);
    }
}