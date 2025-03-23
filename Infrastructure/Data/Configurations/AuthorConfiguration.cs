using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Entities;

namespace Server.Infrastructure.Data.Configurations;

public class AuthorConfiguration : IEntityTypeConfiguration<Author>
{
    public void Configure(EntityTypeBuilder<Author> builder)
    {
        builder.HasKey(a => a.AuthorId);
        
        builder.Property(a => a.Name)
            .IsRequired()
            .HasMaxLength(100);
            
        builder.Property(a => a.Biography)
            .HasMaxLength(5000);
            
        builder.HasMany(a => a.Books)
            .WithMany(b => b.Authors)
            .UsingEntity(j => j.ToTable("BookAuthor"));
    }
}