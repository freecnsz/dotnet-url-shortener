namespace UrlShortener.Application.Abstractions.Services;

public interface IManageTokenService
{
    ManageToken Generate();
    bool Verify(string? token, byte[] storedHash);
}
