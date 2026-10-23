using System.Reflection;
using Microsoft.Extensions.DependencyInjection;
using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Features.ShortUrls.ShortenUrl;
using UrlShortener.Application.Options;

namespace UrlShortener.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddCustomMediator(typeof(ShortenUrlCommand).Assembly);

        services.AddOptions<ShortUrlPolicyOptions>()
            .BindConfiguration(ShortUrlPolicyOptions.SectionName)
            .Validate(o => o.DefaultLifetime > TimeSpan.Zero, "DefaultLifetime must be positive.")
            .Validate(o => o.StatsRetentionAfterExpiry >= TimeSpan.Zero, "StatsRetentionAfterExpiry must not be negative.")
            .ValidateOnStart();

        services.AddSingleton(TimeProvider.System);

        return services;
    }

    private static IServiceCollection AddCustomMediator(this IServiceCollection services, Assembly assembly)
    {
        services.AddScoped<ISender, Mediator>();

        var handlerDef = typeof(IRequestHandler<,>);

        var registrations = assembly.GetTypes()
            .Where(t => t is { IsAbstract: false, IsInterface: false })
            .SelectMany(t => t.GetInterfaces()
                .Where(i => i.IsGenericType &&
                            i.GetGenericTypeDefinition() == handlerDef)
                .Select(i => (Service: i, Implementation: t)));

        foreach (var (service, impl) in registrations)
            services.AddScoped(service, impl);

        return services;
    }
}
