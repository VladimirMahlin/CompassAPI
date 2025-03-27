using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.Application.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;

namespace Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BookshelfController(AppDbContext context, ILogger<BookshelfController> logger) : ControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IEnumerable<BookshelfDto>>> GetBookshelves()
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelves = await context.Bookshelves
            .Where(b => b.UserId == userId)
            .Select(b => new BookshelfDto
            {
                BookshelfId = b.BookshelfId,
                Name = b.Name,
                Description = b.Description,
                IsDefault = b.IsDefault,
                BookCount = b.BookshelfItems.Count
            })
            .ToListAsync();

        return Ok(bookshelves);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<BookshelfDetailDto>> GetBookshelf(int id)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = await context.Bookshelves
            .Include(b => b.BookshelfItems)
            .ThenInclude(bi => bi.Book) 
            .ThenInclude(b => b.Authors)
            .FirstOrDefaultAsync(b => b.BookshelfId == id && b.UserId == userId);

        if (bookshelf == null)
        {
            return NotFound();
        }

        var bookshelfDetail = new BookshelfDetailDto
        {
            BookshelfId = bookshelf.BookshelfId,
            Name = bookshelf.Name,
            Description = bookshelf.Description,
            IsDefault = bookshelf.IsDefault,
            Books = bookshelf.BookshelfItems
                .OrderByDescending(bi => bi.AddedAt)
                .Select(bi => new BookshelfBookDto
                {
                    BookId = bi.Book.BookId,
                    Title = bi.Book.Title,
                    Authors = bi.Book.Authors.Select(a => a.Name).ToList(),
                    CoverImageUrl = bi.Book.CoverImageUrl,
                    AddedAt = bi.AddedAt
                }).ToList()
        };

        return Ok(bookshelfDetail);
    }

    [HttpPost]
    public async Task<ActionResult<BookshelfDto>> CreateBookshelf(CreateBookshelfDto createBookshelfDto)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = new Bookshelf
        {
            Name = createBookshelfDto.Name,
            Description = createBookshelfDto.Description,
            IsDefault = createBookshelfDto.IsDefault,
            UserId = userId,
            BookshelfItems = new List<BookshelfItem>()
        };

        if (bookshelf.IsDefault)
        {
            var existingDefaultBookshelves = await context.Bookshelves
                .Where(b => b.UserId == userId && b.IsDefault)
                .ToListAsync();

            foreach (var existingDefault in existingDefaultBookshelves)
            {
                existingDefault.IsDefault = false;
            }
        }

        context.Bookshelves.Add(bookshelf);
        await context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetBookshelf), new { id = bookshelf.BookshelfId }, new BookshelfDto
        {
            BookshelfId = bookshelf.BookshelfId,
            Name = bookshelf.Name,
            Description = bookshelf.Description,
            IsDefault = bookshelf.IsDefault,
            BookCount = 0
        });
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateBookshelf(int id, UpdateBookshelfDto updateBookshelfDto)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = await context.Bookshelves
            .FirstOrDefaultAsync(b => b.BookshelfId == id && b.UserId == userId);

        if (bookshelf == null)
        {
            return NotFound();
        }

        bookshelf.Name = updateBookshelfDto.Name ?? bookshelf.Name;
        bookshelf.Description = updateBookshelfDto.Description ?? bookshelf.Description;

        if (updateBookshelfDto.IsDefault.HasValue && updateBookshelfDto.IsDefault.Value != bookshelf.IsDefault)
        {
            bookshelf.IsDefault = updateBookshelfDto.IsDefault.Value;

            if (bookshelf.IsDefault)
            {
                var otherDefaultBookshelves = await context.Bookshelves
                    .Where(b => b.UserId == userId && b.IsDefault && b.BookshelfId != id)
                    .ToListAsync();

                foreach (var otherDefault in otherDefaultBookshelves)
                {
                    otherDefault.IsDefault = false;
                }
            }
        }

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!BookshelfExists(id, userId))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteBookshelf(int id)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = await context.Bookshelves
            .FirstOrDefaultAsync(b => b.BookshelfId == id && b.UserId == userId);

        if (bookshelf == null)
        {
            return NotFound();
        }

        if (bookshelf.IsDefault)
        {
            return BadRequest("Cannot delete a default bookshelf");
        }

        context.Bookshelves.Remove(bookshelf);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpPost("{bookshelfId}/books/{bookId}")]
    public async Task<IActionResult> AddBookToBookshelf(int bookshelfId, int bookId)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = await context.Bookshelves
            .Include(b => b.BookshelfItems)
            .FirstOrDefaultAsync(b => b.BookshelfId == bookshelfId && b.UserId == userId);

        if (bookshelf == null)
        {
            return NotFound("Bookshelf not found");
        }

        var book = await context.Books.FindAsync(bookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        if (bookshelf.BookshelfItems.Any(bi => bi.BookId == bookId))
        {
            return BadRequest("Book is already in this bookshelf");
        }

        var bookshelfItem = new BookshelfItem
        {
            BookshelfId = bookshelfId,
            BookId = bookId,
            AddedAt = DateTime.UtcNow
        };

        context.BookshelfItems.Add(bookshelfItem);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpDelete("{bookshelfId}/books/{bookId}")]
    public async Task<IActionResult> RemoveBookFromBookshelf(int bookshelfId, int bookId)
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var bookshelf = await context.Bookshelves
            .FirstOrDefaultAsync(b => b.BookshelfId == bookshelfId && b.UserId == userId);

        if (bookshelf == null)
        {
            return NotFound("Bookshelf not found");
        }

        var bookshelfItem = await context.BookshelfItems
            .FirstOrDefaultAsync(bi => bi.BookshelfId == bookshelfId && bi.BookId == bookId);

        if (bookshelfItem == null)
        {
            return NotFound("Book not found in bookshelf");
        }

        context.BookshelfItems.Remove(bookshelfItem);
        await context.SaveChangesAsync();

        return NoContent();
    }

    [HttpGet("default")]
    public async Task<ActionResult<BookshelfDetailDto>> GetDefaultBookshelf()
    {
        int userId = 1; // TODO: Replace with actual user ID from authentication

        var defaultBookshelf = await context.Bookshelves
            .Include(b => b.BookshelfItems)
            .ThenInclude(bi => bi.Book)
            .ThenInclude(b => b.Authors)
            .FirstOrDefaultAsync(b => b.UserId == userId && b.IsDefault);

        if (defaultBookshelf == null)
        {
            return NotFound("No default bookshelf found");
        }

        var bookshelfDetail = new BookshelfDetailDto
        {
            BookshelfId = defaultBookshelf.BookshelfId,
            Name = defaultBookshelf.Name,
            Description = defaultBookshelf.Description,
            IsDefault = true,
            Books = defaultBookshelf.BookshelfItems
                .OrderByDescending(bi => bi.AddedAt)
                .Select(bi => new BookshelfBookDto
                {
                    BookId = bi.Book.BookId,
                    Title = bi.Book.Title,
                    Authors = bi.Book.Authors.Select(a => a.Name).ToList(),
                    CoverImageUrl = bi.Book.CoverImageUrl,
                    AddedAt = bi.AddedAt
                }).ToList()
        };

        return Ok(bookshelfDetail);
    }

    private bool BookshelfExists(int id, int userId)
    {
        return context.Bookshelves.Any(b => b.BookshelfId == id && b.UserId == userId);
    }
}