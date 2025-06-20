using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using EventStoreService.Domain.Entities;

namespace EventStoreService.Infrastructure.Persistence.Configurations;

public class EventConfiguration : IEntityTypeConfiguration<Event>
{
    public void Configure(EntityTypeBuilder<Event> builder)
    {
        builder.ToTable("Events");

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

        // Audit fields
        builder.Property(x => x.CreatedAt)
            .IsRequired();

        builder.Property(x => x.UpdatedAt)
            .IsRequired(false);

        // Ignore domain events
        builder.Ignore(x => x.DomainEvents);
    }
}