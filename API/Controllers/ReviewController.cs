using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.DTOs;
using Server.Core.Entities;
using Server.Infrastructure.Data;

namespace Server.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ReviewController(AppDbContext context, ILogger<ReviewController> logger) : ControllerBase
{
    private readonly ILogger<ReviewController> _logger = logger;

    [HttpGet]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetReviews()
    {
        var reviews = await context.Reviews
            .Include(r => r.User)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Username
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<ReviewDto>> GetReview(int id)
    {
        var review = await context.Reviews
            .Include(r => r.User)
            .Include(r => r.Book)
            .FirstOrDefaultAsync(r => r.ReviewId == id);

        if (review == null)
        {
            return NotFound();
        }

        var reviewDto = new ReviewDto
        {
            ReviewId = review.ReviewId,
            Rating = review.Rating,
            Content = review.Content,
            CreatedAt = review.CreatedAt,
            UserName = review.User.Username
        };

        return Ok(reviewDto);
    }

    [HttpGet("book/{bookId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetBookReviews(int bookId)
    {
        var book = await context.Books.FindAsync(bookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        var reviews = await context.Reviews
            .Include(r => r.User)
            .Where(r => r.BookId == bookId)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Username
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpGet("user/{userId}")]
    public async Task<ActionResult<IEnumerable<ReviewDto>>> GetUserReviews(int userId)
    {
        var user = await context.Users.FindAsync(userId);
        if (user == null)
        {
            return NotFound("User not found");
        }

        var reviews = await context.Reviews
            .Include(r => r.User)
            .Where(r => r.UserId == userId)
            .Select(r => new ReviewDto
            {
                ReviewId = r.ReviewId,
                Rating = r.Rating,
                Content = r.Content,
                CreatedAt = r.CreatedAt,
                UserName = r.User.Username
            })
            .ToListAsync();

        return Ok(reviews);
    }

    [HttpPost]
    public async Task<ActionResult<ReviewDto>> CreateReview(CreateReviewDto createReviewDto)
    {
        int userId = 1; //TODO Replace with actual user ID from authentication

        var book = await context.Books.FindAsync(createReviewDto.BookId);
        if (book == null)
        {
            return NotFound("Book not found");
        }

        // Check if user has already reviewed this book
        var existingReview = await context.Reviews
            .FirstOrDefaultAsync(r => r.BookId == createReviewDto.BookId && r.UserId == userId);

        if (existingReview != null)
        {
            return BadRequest("User has already reviewed this book");
        }

        var review = new Review
        {
            Rating = createReviewDto.Rating,
            Content = createReviewDto.Content,
            BookId = createReviewDto.BookId,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        context.Reviews.Add(review);
        await context.SaveChangesAsync();

        var username = await context.Users
            .Where(u => u.UserId == userId)
            .Select(u => u.Username)
            .FirstOrDefaultAsync();

        var reviewDto = new ReviewDto
        {
            ReviewId = review.ReviewId,
            Rating = review.Rating,
            Content = review.Content,
            CreatedAt = review.CreatedAt,
            UserName = username
        };

        return CreatedAtAction(nameof(GetReview), new { id = review.ReviewId }, reviewDto);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateReview(int id, UpdateReviewDto updateReviewDto)
    {
        int userId = 1; //TODO Replace with actual user ID from authentication

        var review = await context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }
        
        if (review.UserId != userId)
        {
            return Forbid("You can only update your own reviews");
        }

        review.Rating = updateReviewDto.Rating;
        review.Content = updateReviewDto.Content;
        review.UpdatedAt = DateTime.UtcNow;

        try
        {
            await context.SaveChangesAsync();
        }
        catch (DbUpdateConcurrencyException)
        {
            if (!ReviewExists(id))
            {
                return NotFound();
            }

            throw;
        }

        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteReview(int id)
    {
        int userId = 1; //TODO Replace with actual user ID from authentication

        var review = await context.Reviews.FindAsync(id);
        if (review == null)
        {
            return NotFound();
        }

        if (review.UserId != userId)
        {
            return Forbid("You can only delete your own reviews");
        }

        context.Reviews.Remove(review);
        await context.SaveChangesAsync();

        return NoContent();
    }

    private bool ReviewExists(int id)
    {
        return context.Reviews.Any(e => e.ReviewId == id);
    }
}