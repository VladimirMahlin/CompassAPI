using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Entities;

namespace Server.Infrastructure.Data.Configurations;

public class BookshelfBookConfiguration : IEntityTypeConfiguration<BookshelfItem>
{
    public void Configure(EntityTypeBuilder<BookshelfItem> builder)
    {
        builder.HasKey(bb => new { bb.BookshelfId, bb.BookId });
        
        builder.Property(bb => bb.AddedAt)
            .IsRequired()
            .HasDefaultValueSql("CURRENT_TIMESTAMP");
            
        builder.HasOne(bb => bb.Bookshelf)
            .WithMany(b => b.BookshelfItems)
            .HasForeignKey(bb => bb.BookshelfId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasOne(bb => bb.Book)
            .WithMany()
            .HasForeignKey(bb => bb.BookId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}