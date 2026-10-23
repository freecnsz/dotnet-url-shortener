using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Npgsql;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Domain.Entities;
using UrlShortener.Infrastructure.Persistence.Configurations;
using UrlShortener.Infrastructure.Persistence.Identity;

namespace UrlShortener.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options)
    : IdentityDbContext<AppUser, IdentityRole<Guid>, Guid>(options), IUnitOfWork
{
    public DbSet<ShortUrl> ShortUrls { get; set; } = null!;
    public DbSet<ClickEvent> ClickEvents { get; set; } = null!;

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Identity defaults first, so our configurations can override them
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
    }

    public override async Task<int> SaveChangesAsync(
        bool acceptAllChangesOnSuccess,
        CancellationToken cancellationToken = default)
    {
        try
        {
            return await base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
        }
        catch (DbUpdateException ex) when (ex.InnerException is PostgresException
        {
            SqlState: PostgresErrorCodes.UniqueViolation,
            ConstraintName: ShortUrlConfiguration.ShortCodeIndexName
        })
        {
            throw new DuplicateShortCodeException(ex);
        }
    }
}
