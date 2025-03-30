namespace Server.API.DTOs;

public class ReviewDto
{
    public int ReviewId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
    public DateTime CreatedAt { get; set; }
    public string UserName { get; set; }
}

public class CreateReviewDto
{
    public int BookId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
}

public class UpdateReviewDto
{
    public int Rating { get; set; }
    public string Content { get; set; }
}