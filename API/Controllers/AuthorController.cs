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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
    {
        try
        {
            var authors = await context.Authors
                .AsNoTracking()
                .Select(a => new AuthorDto
                {
                    AuthorId = a.AuthorId,
                    Name = a.Name,
                    Biography = a.Biography
                })
                .ToListAsync();

            return Ok(authors);
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all authors");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<AuthorDetailDto>> GetAuthor(int id)
    {
        try
        {
            var author = await context.Authors
                .AsNoTracking()
                .Include(a => a.Books)
                .FirstOrDefaultAsync(a => a.AuthorId == id);

            if (author == null)
            {
                return NotFound("Author not found");
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting author");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto createAuthorDto)
    {
        try
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating author");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateAuthor(int id, UpdateAuthorDto updateAuthorDto)
    {
        try
        {
            var author = await context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            author.Name = updateAuthorDto.Name;
            author.Biography = updateAuthorDto.Biography;
            
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!AuthorExists(id))
            {
                return NotFound("Author not found");
                
            }
            return StatusCode(500, "Internal Server Error - Concurrency conflict");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating author");
            return StatusCode(500, "Internal Server Error");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteAuthor(int id)
    {
        try
        {
            var author = await context.Authors.FindAsync(id);
            if (author == null)
            {
                return NotFound();
            }

            context.Authors.Remove(author);
            await context.SaveChangesAsync();
        }
        
        catch (Exception ex) 
        {
            logger.LogError(ex, "Error deleting author");
            return StatusCode(500, "Internal Server Error");
        }
        
        return NoContent();
    }

    [HttpGet("{id}/books")]
    public async Task<ActionResult<IEnumerable<BookBriefDto>>> GetAuthorBooks(int id)
    {
        try
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
        catch (Exception ex) 
        {
            logger.LogError(ex, "Error getting author's books");
            return StatusCode(500, "Internal Server Error");
        }
    }

    private bool AuthorExists(int id)
    {
        return context.Authors.Any(e => e.AuthorId == id);
    }
}