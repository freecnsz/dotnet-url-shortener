using Hangfire;
using Hangfire.PostgreSql;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StackExchange.Redis;
using UrlShortener.Application.Abstractions.Persistence;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Infrastructure.ClickTracking;
using UrlShortener.Infrastructure.Jobs;
using UrlShortener.Infrastructure.Options;
using UrlShortener.Infrastructure.Persistence;
using UrlShortener.Infrastructure.Persistence.Repositories;
using UrlShortener.Infrastructure.Services;

namespace UrlShortener.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructureServices(
        this IServiceCollection services, IConfiguration configuration)
    {
        var dbConnectionString = configuration.GetConnectionString("DefaultConnection");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(dbConnectionString));
        services.AddScoped<IUnitOfWork>(sp => sp.GetRequiredService<AppDbContext>());
        services.AddScoped<IShortUrlRepository, ShortUrlRepository>();
        services.AddScoped<IClickRepository, ClickRepository>();

        services.AddHangfire(config =>
            config.UsePostgreSqlStorage(options => options.UseNpgsqlConnection(dbConnectionString)));
        services.AddHangfireServer();
        services.AddScoped<ExpiredLinkCleanupJob>();

        services.AddSingleton<IConnectionMultiplexer>(_ =>
            ConnectionMultiplexer.Connect(configuration.GetConnectionString("RedisConnection")!));
        services.AddScoped<IUrlLookup, UrlLookup>();

        services.AddSingleton<IAbsoluteUrlBuilder, AbsoluteUrlBuilder>();
        services.AddSingleton<IQRCodeGenerator, CustomQRCodeGenerator>();
        services.AddSingleton<IShortCodeGenerator, ShortCodeGenerator>();
        services.AddSingleton<IManageTokenService, ManageTokenService>();

        // One queue instance: the recorder writes to it, the background writer drains it
        services.AddSingleton<ClickQueue>();
        services.AddSingleton<IClickRecorder>(sp => sp.GetRequiredService<ClickQueue>());
        services.AddHostedService<ClickWriter>();

        services.AddOptions<ShortUrlOptions>()
            .BindConfiguration(ShortUrlOptions.SectionName)
            .Validate(
                o => Uri.TryCreate(o.BaseUrl, UriKind.Absolute, out var uri)
                     && (uri.Scheme == Uri.UriSchemeHttps || uri.Scheme == Uri.UriSchemeHttp)
                     && uri.AbsolutePath == "/"
                     && string.IsNullOrEmpty(uri.Query),
                "BaseUrl must be an absolute http(s) URL with no path or query, e.g. https://example.com")
            .ValidateOnStart();

        services.AddOptions<UrlCacheOptions>()
            .BindConfiguration(UrlCacheOptions.SectionName);

        return services;
    }
}
