using System.ComponentModel.DataAnnotations;

namespace Server.Core.Entities;

public class Review
{
    public int ReviewId { get; set; }
    public int Rating { get; set; }
    public string Content { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    public int BookId { get; set; }
    public int UserId { get; set; }
    
    public Book Book { get; set; }
    public User User { get; set; }
}