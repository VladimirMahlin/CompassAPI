using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;

namespace Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GenreController(AppDbContext context, ILogger<GenreController> logger) : ControllerBase
{
    private readonly ILogger<GenreController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<GenreDto>>> GetGenres()
    {
        var genres = await context.Genres
            .Select(g => new GenreDto
            {
                GenreId = g.GenreId,
                Name = g.Name,
                Description = g.Description
            })
            .ToListAsync();

        return Ok(genres);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<GenreDetailDto>> GetGenre(int id)
    {
        var genre = await context.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.GenreId == id);

        if (genre == null)
        {
            return NotFound();
        }

        var genreDetail = new GenreDetailDto
        {
            GenreId = genre.GenreId,
            Name = genre.Name,
            Description = genre.Description,
            Books = genre.Books.Select(b => new BookBriefDto
            {
                BookId = b.BookId,
                Title = b.Title,
                CoverImageUrl = b.CoverImageUrl
            }).ToList()
        };

        return Ok(genreDetail);
    }

    [HttpPost]
    public async Task<ActionResult<GenreDto>> CreateGenre(CreateGenreDto createGenreDto)
    {
        var genre = new Genre
        {
            Name = createGenreDto.Name,
            Description = createGenreDto.Description,
            Books = new List<Book>()
        };

        context.Genres.Add(genre);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetGenre), new { id = genre.GenreId }, new GenreDto
        {
            GenreId = genre.GenreId,
            Name = genre.Name,
            Description = genre.Description
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateGenre(int id, UpdateGenreDto updateGenreDto)
    {
        var genre = await context.Genres.FindAsync(id);
        if (genre == null)
        {
            return NotFound();
        }

        genre.Name = updateGenreDto.Name ?? genre.Name;
        genre.Description = updateGenreDto.Description ?? genre.Description;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!GenreExists(id))
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
    public async Task<IActionResult> DeleteGenre(int id)
    {
        var genre = await context.Genres.FindAsync(id);
        if (genre == null)
        {
            return NotFound();
        }

        context.Genres.Remove(genre);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{genreId}/books/{bookId}")]
    public async Task<IActionResult> AddBookToGenre(int genreId, int bookId)
    {
        var genre = await context.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.GenreId == genreId);

        if (genre == null)
        {
            return NotFound("Genre not found");
        }

        var book = await context.Books.FindAsync(bookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        if (genre.Books.Any(b => b.BookId == bookId))
        {
            return BadRequest("Book is already in this genre");
        }

        genre.Books.Add(book);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{genreId}/books/{bookId}")]
    public async Task<IActionResult> RemoveBookFromGenre(int genreId, int bookId)
    {
        var genre = await context.Genres
            .Include(g => g.Books)
            .FirstOrDefaultAsync(g => g.GenreId == genreId);

        if (genre == null)
        {
            return NotFound("Genre not found");
        }

        var book = genre.Books.FirstOrDefault(b => b.BookId == bookId);
        if (book == null)
        {
            return NotFound("Book not found in genre");
        }

        genre.Books.Remove(book);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool GenreExists(int id)
    {
        return context.Genres.Any(e => e.GenreId == id);
    }
}