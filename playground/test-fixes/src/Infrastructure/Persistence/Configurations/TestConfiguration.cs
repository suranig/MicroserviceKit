using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TestService.Domain.Test;

namespace TestService.Infrastructure.Persistence.Configurations;

public class TestConfiguration : IEntityTypeConfiguration<Test>
{
    public void Configure(EntityTypeBuilder<Test> builder)
    {
        builder.ToTable("Tests");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Id)
            .IsRequired(true);
        builder.Property(x => x.Name)
            .HasMaxLength(255)
            .IsRequired(true);
        builder.Property(x => x.Description)
            .HasMaxLength(255)
            .IsRequired(false);
        builder.Property(x => x.CreatedAt)
            .IsRequired(true);
        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
    }
}