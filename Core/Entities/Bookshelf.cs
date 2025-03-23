namespace Server.Core.Entities;

public class Bookshelf
{
    public int BookshelfId { get; set; }
    public string Name { get; set; }
    public string Description { get; set; }
    public bool IsDefault { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; }
    
    public ICollection<BookshelfItem> BookshelfItems { get; set; }
}