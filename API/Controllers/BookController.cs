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
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
    {
        try
        {
            var books = await context.Books
                .AsNoTracking()
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting all books");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookDetailDto>> GetBook(int id)
    {
        try
        {
            var book = await context.Books
                .AsNoTracking()
                .Include(b => b.Authors)
                .Include(b => b.Reviews)
                .ThenInclude(r => r.User)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound("Book not found");
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error getting book with ID: {BookId}", id);
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPost]
    public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto createBookDto)
    {
        try
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

            foreach (var authorName in createBookDto.AuthorNames)
            {
                var existingAuthor = await context.Authors.FirstOrDefaultAsync(a => a.Name == authorName);
                if (existingAuthor != null)
                {
                    book.Authors.Add(existingAuthor);
                }
                else
                {
                    var newAuthor = new Author { Name = authorName, Biography = ""};
                    context.Authors.Add(newAuthor);
                    book.Authors.Add(newAuthor);
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
        catch (Exception ex)
        {
            logger.LogError(ex, "Error creating book");
            return StatusCode(500, "Internal Server Error");
        }
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBook(int id, UpdateBookDto updateBookDto)
    {
        try
        {
            var book = await context.Books
                .Include(b => b.Authors)
                .FirstOrDefaultAsync(b => b.BookId == id);

            if (book == null)
            {
                return NotFound("Book not found");
            }

            book.Title = updateBookDto.Title;
            book.ISBN = updateBookDto.ISBN;
            book.PublicationYear = updateBookDto.PublicationYear;
            book.Publisher = updateBookDto.Publisher;
            book.CoverImageUrl = updateBookDto.CoverImageUrl;
            book.Description = updateBookDto.Description;

            book.Authors.Clear();

            var authors = await context.Authors
                .Where(a => updateBookDto.AuthorIds.Contains(a.AuthorId))
                .ToListAsync();

            foreach (var author in authors)
            {
                book.Authors.Add(author);
            }

            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookExists(id))
            {
                return NotFound("Book not found");
            }

            return StatusCode(500, "Internal Server Error - Concurrency conflict");
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error updating book with ID: {BookId}", id);
            return StatusCode(500, "Internal Server Error");
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBook(int id)
    {
        try
        {
            var book = await context.Books.FindAsync(id);
            if (book == null)
            {
                return NotFound("Book not found");
            }

            context.Books.Remove(book);
            await context.SaveChangesAsync();

            return NoContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Error deleting book with ID: {BookId}", id);
            return StatusCode(500, "Internal Server Error");
        }
    }

    private bool BookExists(int id)
    {
        return context.Books.Any(e => e.BookId == id);
    }
}