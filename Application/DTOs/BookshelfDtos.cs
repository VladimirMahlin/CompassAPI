namespace Server.Application.DTOs;

public class BookshelfDto
{
    public int BookshelfId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
    public int BookCount { get; set; }
}

public class BookshelfDetailDto
{
    public int BookshelfId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
    public List<BookshelfBookDto> Books { get; set; }
}

public class BookshelfBookDto
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public List<string> Authors { get; set; }
    public string CoverImageUrl { get; set; }
    public DateTime AddedAt { get; set; }
}

public class CreateBookshelfDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
}

public class UpdateBookshelfDto
{
    public string Name { get; set; }
    public string Description { get; set; }
    public bool? IsDefault { get; set; }
}