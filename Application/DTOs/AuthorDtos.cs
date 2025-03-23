namespace Server.API.DTOs;

public class AuthorDto
{
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Biography { get; set; }
}

public class AuthorDetailDto
{
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Biography { get; set; }
    public List<BookBriefDto> Books { get; set; }
}

public class CreateAuthorDto
{
    public string Name { get; set; }
    public string Biography { get; set; }
}

public class UpdateAuthorDto
{
    public string Name { get; set; }
    public string Biography { get; set; }
}