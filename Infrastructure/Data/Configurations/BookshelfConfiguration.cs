using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Entities;

namespace Server.Infrastructure.Data.Configurations;

public class BookshelfConfiguration : IEntityTypeConfiguration<Bookshelf>
{
    public void Configure(EntityTypeBuilder<Bookshelf> builder)
    {
        builder.HasKey(b => b.BookshelfId);
        
        builder.Property(b => b.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(b => b.Description)
            .HasMaxLength(500);
            
        builder.HasOne(b => b.User)
            .WithMany()
            .HasForeignKey(b => b.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}