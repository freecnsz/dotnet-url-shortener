namespace UrlShortener.Application.Abstractions.Persistence;

public sealed class DuplicateShortCodeException(Exception innerException)
    : Exception("Short code already exists.", innerException);
