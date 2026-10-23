namespace UrlShortener.Application.Abstractions.Services;

public interface IQRCodeGenerator
{
    byte[] GeneratePngQRCode(string url);
}
