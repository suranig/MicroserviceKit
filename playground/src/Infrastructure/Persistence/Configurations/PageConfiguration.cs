using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using ReadModelService.Domain.Page;

namespace ReadModelService.Infrastructure.Persistence.Configurations;

public class PageConfiguration : IEntityTypeConfiguration<Page>
{
    public void Configure(EntityTypeBuilder<Page> builder)
    {
        builder.ToTable("Pages");

        builder.HasKey(x => x.Id);

        builder.Property(x => x.Id)
            .ValueGeneratedNever();

        builder.Property(x => x.Id)
            .IsRequired(true);
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