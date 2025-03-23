using Microsoft.EntityFrameworkCore;
using Server.Core.Entities;

namespace Server.Infrastructure.Data;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<Book> Books { get; set; }
    public DbSet<Author> Authors { get; set; }
    public DbSet<Review> Reviews { get; set; }
    public DbSet<User> Users { get; set; }
    
    public DbSet<Genre> Genres { get; set; }
    public DbSet<Bookshelf> Bookshelves { get; set; }
    public DbSet<BookshelfItem> BookshelfItems { get; set; }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }
}