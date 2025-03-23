namespace Server.API.DTOs;

public class BookDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string Publisher { get; set; }
    public string CoverImageUrl { get; set; }
    public string Description { get; set; }
    public List<string> Authors { get; set; }
}

public class BookDetailDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string Publisher { get; set; }
    public string CoverImageUrl { get; set; }
    public string Description { get; set; }
    public List<AuthorDto> Authors { get; set; }
    public List<ReviewDto> Reviews { get; set; }
}

public class BookBriefDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string CoverImageUrl { get; set; }
    public int PublicationYear { get; set; }
}

public class CreateBookDto
{
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string Publisher { get; set; }
    public string CoverImageUrl { get; set; }
    public string Description { get; set; }
    public List<int> AuthorIds { get; set; }
}

public class UpdateBookDto
{
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int? PublicationYear { get; set; }
    public string Publisher { get; set; }
    public string CoverImageUrl { get; set; }
    public string Description { get; set; }
    public List<int> AuthorIds { get; set; }
}