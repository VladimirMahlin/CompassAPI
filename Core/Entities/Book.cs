using System.ComponentModel.DataAnnotations;

namespace Server.Core.Entities;

public class Book
{
    public int BookId { get; set; }
    public string Title { get; set; }
    public string ISBN { get; set; }
    public int PublicationYear { get; set; }
    public string Publisher { get; set; }
    public string CoverImageUrl { get; set; }
    public string Description { get; set; }
    
    public ICollection<Author> Authors { get; set; }
    public ICollection<Review> Reviews { get; set; }
}