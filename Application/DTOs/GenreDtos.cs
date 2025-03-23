namespace Server.API.DTOs;

public class GenreDto
{
    public int GenreId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
}

public class GenreDetailDto
{
    public int GenreId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public List<BookBriefDto> Books { get; set; }
}

public class CreateGenreDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}

public class UpdateGenreDto
{
    public string Name { get; set; }
    public string Description { get; set; }
}