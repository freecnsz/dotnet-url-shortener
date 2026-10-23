using QRCoder;
using UrlShortener.Application.Abstractions.Services;

namespace UrlShortener.Infrastructure.Services;

public sealed class CustomQRCodeGenerator : IQRCodeGenerator
{
    private const int PixelsPerModule = 20;

    public byte[] GeneratePngQRCode(string url)
    {
        using var generator = new QRCodeGenerator();
        var data = generator.CreateQrCode(url, QRCodeGenerator.ECCLevel.Q);
        return new PngByteQRCode(data).GetGraphic(PixelsPerModule);
    }
}
