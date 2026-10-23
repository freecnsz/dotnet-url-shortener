using System.Security.Cryptography;
using UrlShortener.Application.Abstractions.Services;
using UrlShortener.Domain.Entities;

namespace UrlShortener.Infrastructure.Services;

public sealed class ShortCodeGenerator : IShortCodeGenerator
{
    private const string Characters =
        "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";

    public string Generate()
        => RandomNumberGenerator.GetString(Characters, ShortUrl.ShortCodeLength);
}
