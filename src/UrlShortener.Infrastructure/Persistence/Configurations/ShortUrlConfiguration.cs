using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using UrlShortener.Domain.Entities;
using UrlShortener.Domain.ValueObjects;

namespace UrlShortener.Infrastructure.Persistence.Configurations;

public sealed class ShortUrlConfiguration : IEntityTypeConfiguration<ShortUrl>
{
    public const string ShortCodeIndexName = "IX_ShortUrls_ShortCode";

    public void Configure(EntityTypeBuilder<ShortUrl> builder)
    {
        builder.ToTable("ShortUrls", t =>
        {
            t.HasCheckConstraint(
                "CK_ShortUrls_ShortCode_Length",
                $"char_length(\"ShortCode\") = {ShortUrl.ShortCodeLength}");

            // bytea has no length modifier, so length is enforced here
            t.HasCheckConstraint(
                "CK_ShortUrls_ManageTokenHash_Length",
                $"octet_length(\"ManageTokenHash\") = {ShortUrl.ManageTokenHashLength}");

            t.HasCheckConstraint(
                "CK_ShortUrls_ExpiresAfterCreated",
                "\"ExpiresAtUtc\" > \"CreatedAtUtc\"");
        });

        builder.HasKey(e => e.Id);
        builder.Property(e => e.Id)
            .ValueGeneratedNever(); // UUID v7 is generated in the domain

        builder.Property(e => e.OriginalUrl)
            .HasConversion(
                vo => vo.Value,
                value => OriginalUrl.Create(value))
            .HasMaxLength(OriginalUrl.MaxLength)
            .IsRequired();

        builder.Property(e => e.ShortCode)
            .HasMaxLength(ShortUrl.ShortCodeLength)
            .IsRequired();

        builder.HasIndex(e => e.ShortCode)
            .IsUnique()
            .HasDatabaseName(ShortCodeIndexName);

        builder.Property(e => e.ManageTokenHash)
            .IsRequired();

        builder.Property(e => e.CreatedAtUtc)
            .IsRequired();

        builder.Property(e => e.ExpiresAtUtc)
            .IsRequired();
    }
}
