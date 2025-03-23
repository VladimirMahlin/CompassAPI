namespace Server.Core.Entities;

public class Genre
{
    public int GenreId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }

    public ICollection<Book> Books { get; set; }
}