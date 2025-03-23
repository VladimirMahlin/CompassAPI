using System.ComponentModel.DataAnnotations;

namespace Server.Core.Entities;

public class User
{
    public int UserId { get; set; }
    public string Username { get; set; }
    public string Email { get; set; }
    public string PasswordHash { get; set; }
    public DateTime JoinedAt { get; set; }
    public string ProfilePictureUrl { get; set; }
    public string Bio { get; set; }
    
    public ICollection<Review> Reviews { get; set; }
}