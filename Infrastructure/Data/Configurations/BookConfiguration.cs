using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Entities;

namespace Server.Infrastructure.Data.Configurations;

public class BookConfiguration : IEntityTypeConfiguration<Book>
{
    public void Configure(EntityTypeBuilder<Book> builder)
    {
        builder.HasKey(b => b.BookId);
        
        builder.Property(b => b.Title)
            .IsRequired()
            .HasMaxLength(200);
            
        builder.Property(b => b.ISBN)
            .HasMaxLength(13);
            
        builder.Property(b => b.Description)
            .HasMaxLength(5000);
        
        builder.HasMany(b => b.Reviews)
            .WithOne(r => r.Book)
            .HasForeignKey(r => r.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}