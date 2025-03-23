using System.ComponentModel.DataAnnotations;

namespace Server.Core.Entities;

public class Author
{
    public int AuthorId { get; set; }
    public string Name { get; set; }
    public string Biography { get; set; }
    
    public ICollection<Book> Books { get; set; }
}