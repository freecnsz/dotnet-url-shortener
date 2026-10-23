using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

public sealed class ClickEventConfiguration : IEntityTypeConfiguration<ClickEvent>
{
    public void Configure(EntityTypeBuilder<ClickEvent> builder)
    {
        builder.ToTable("ClickEvents");

        builder.HasKey(e => e.Id);

        builder.HasOne<ShortUrl>()
            .WithMany()
            .HasForeignKey(e => e.ShortUrlId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(e => new { e.ShortUrlId, e.OccurredAtUtc });

        builder.Property(e => e.OccurredAtUtc).IsRequired();

        builder.Property(e => e.CountryCode)
            .HasMaxLength(ClickEvent.CountryCodeLength)
            .IsFixedLength();

        builder.Property(e => e.RefererHost)
            .HasMaxLength(ClickEvent.RefererHostMaxLength);

        builder.Property(e => e.UserAgent)
            .HasMaxLength(ClickEvent.UserAgentMaxLength);
    }
}
