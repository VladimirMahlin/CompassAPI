using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Server.Core.Entities;

namespace Server.Infrastructure.Data.Configurations;

public class GenreConfiguration : IEntityTypeConfiguration<Genre>
{
    public void Configure(EntityTypeBuilder<Genre> builder)
    {
        builder.HasKey(g => g.GenreId);
        
        builder.Property(g => g.Name)
            .IsRequired()
            .HasMaxLength(50);
            
        builder.Property(g => g.Description)
            .HasMaxLength(1000);
            
        builder.HasMany(g => g.Books)
            .WithMany()
            .UsingEntity(j => j.ToTable("BookGenre"));
    }
}