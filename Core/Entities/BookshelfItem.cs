namespace Server.Core.Entities;

public class BookshelfItem
{
    public int BookshelfId { get; set; }
    public int BookId { get; set; }
    
    public DateTime AddedAt { get; set; }
    
    public Bookshelf Bookshelf { get; set; }
    public Book Book { get; set; }
}