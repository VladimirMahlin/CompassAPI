using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;

namespace Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookController(AppDbContext context, ILogger<BookController> logger) : ControllerBase
{
    private readonly ILogger<BookController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        var books = await context.Books
            .Include(b => b.Authors)
            .Select(b => new BookDto
            {
                BookId = b.BookId,
                Title = b.Title,
                ISBN = b.ISBN,
                PublicationYear = b.PublicationYear,
                Publisher = b.Publisher,
                CoverImageUrl = b.CoverImageUrl,
                Description = b.Description,
                Authors = b.Authors.Select(a => a.Name).ToList()
            })
            .ToListAsync();

        return Ok(books);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailDto>> GetBook(int id)
    {
        var book = await context.Books
            .Include(b => b.Authors)
            .Include(b => b.Reviews)
            .ThenInclude(r => r.User)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null)
        {
            return NotFound();
        }

        var bookDetail = new BookDetailDto
        {
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            Publisher = book.Publisher,
            CoverImageUrl = book.CoverImageUrl,
            Description = book.Description,
            Authors = book.Authors.Select(a => new AuthorDto
            {
                AuthorId = a.AuthorId,
                Name = a.Name
            }).ToList(),
            Reviews = book.Reviews.Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Username
            }).ToList()
        };

        return Ok(bookDetail);
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        var book = new Book
        {
            Title = createBookDto.Title,
            ISBN = createBookDto.ISBN,
            PublicationYear = createBookDto.PublicationYear,
            Publisher = createBookDto.Publisher,
            CoverImageUrl = createBookDto.CoverImageUrl,
            Description = createBookDto.Description,
            Authors = new List<Author>()
        };

        if (createBookDto.AuthorIds != null && createBookDto.AuthorIds.Any())
        {
            var authors = await context.Authors
                .Where(a => createBookDto.AuthorIds.Contains(a.AuthorId))
                .ToListAsync();

            foreach (var author in authors)
            {
                book.Authors.Add(author);
            }
        }

        context.Books.Add(book);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBook), new { id = book.BookId }, new BookDto
        {
            BookId = book.BookId,
            Title = book.Title,
            ISBN = book.ISBN,
            PublicationYear = book.PublicationYear,
            Publisher = book.Publisher,
            CoverImageUrl = book.CoverImageUrl,
            Description = book.Description,
            Authors = book.Authors.Select(a => a.Name).ToList()
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        var book = await context.Books
            .Include(b => b.Authors)
            .FirstOrDefaultAsync(b => b.BookId == id);

        if (book == null)
        {
            return NotFound();
        }

        book.Title = updateBookDto.Title ?? book.Title;
        book.ISBN = updateBookDto.ISBN ?? book.ISBN;
        book.PublicationYear = updateBookDto.PublicationYear ?? book.PublicationYear;
        book.Publisher = updateBookDto.Publisher ?? book.Publisher;
        book.CoverImageUrl = updateBookDto.CoverImageUrl ?? book.CoverImageUrl;
        book.Description = updateBookDto.Description ?? book.Description;

        if (updateBookDto.AuthorIds != null)
        {
            book.Authors.Clear();

            var authors = await context.Authors
                .Where(a => updateBookDto.AuthorIds.Contains(a.AuthorId))
                .ToListAsync();

            foreach (var author in authors)
            {
                book.Authors.Add(author);
            }
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        var book = await context.Books.FindAsync(id);
        if (book == null)
        {
            return NotFound();
        }

        context.Books.Remove(book);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool BookExists(int id)
    {
        return context.Books.Any(e => e.BookId == id);
    }
}