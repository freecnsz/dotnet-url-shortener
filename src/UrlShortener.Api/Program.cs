using Hangfire;
using Microsoft.AspNetCore.HttpOverrides;
using Scalar.AspNetCore;
using Serilog;
using UrlShortener.Api.Extensions;
using UrlShortener.Api.Middlewares;
using UrlShortener.Api.Options;
using UrlShortener.Application;
using UrlShortener.Infrastructure;

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, config) => config
    .ReadFrom.Configuration(context.Configuration)
    .Enrich.FromLogContext());

builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto;

    // Trust only proxies on private networks (Docker, nginx); loopback is trusted by default
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("10.0.0.0/8"));
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("172.16.0.0/12"));
    options.KnownIPNetworks.Add(System.Net.IPNetwork.Parse("192.168.0.0/16"));
});

builder.Services.AddProblemDetails(options =>
{
    // Framework-generated errors get a generic code; our own specific codes are never overwritten
    options.CustomizeProblemDetails = context =>
    {
        var extensions = context.ProblemDetails.Extensions;
        if (!extensions.ContainsKey("code"))
            extensions["code"] = FallbackErrorCode(context.ProblemDetails.Status);
    };
});
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

builder.Services.Configure<ClickTrackingOptions>(
    builder.Configuration.GetSection(ClickTrackingOptions.SectionName));

builder.Services.AddInfrastructureServices(builder.Configuration);
builder.Services.AddApplicationServices();

builder.Services.AddLinkCreationRateLimiter();
builder.Services.AddControllers();
builder.Services.AddOpenApi();

builder.Services.AddHealthChecks()
    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection")!,
        name: "PostgreSQL", tags: ["db", "sql", "postgresql"])
    .AddRedis(builder.Configuration.GetConnectionString("RedisConnection")!,
        name: "Redis", tags: ["db", "cache", "redis"]);

var app = builder.Build();

app.UseForwardedHeaders();
app.UseSerilogRequestLogging();
app.UseExceptionHandler();
app.UseStatusCodePages();

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference();
    app.UseHangfireDashboard();
}

app.UseRateLimiter();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.RegisterRecurringJobs();

app.Run();

static string FallbackErrorCode(int? status) => status switch
{
    StatusCodes.Status400BadRequest => "Request.Invalid",
    StatusCodes.Status404NotFound => "Request.NotFound",
    StatusCodes.Status405MethodNotAllowed => "Request.MethodNotAllowed",
    StatusCodes.Status415UnsupportedMediaType => "Request.UnsupportedMediaType",
    >= 500 => "Server.Unexpected",
    _ => "Request.Failed"
};
