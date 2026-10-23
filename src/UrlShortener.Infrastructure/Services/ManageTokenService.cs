using System.Buffers.Text;
using System.Security.Cryptography;
using System.Text;
using UrlShortener.Application.Abstractions.Services;

namespace UrlShortener.Infrastructure.Services;

public sealed class ManageTokenService : IManageTokenService
{
    private const int TokenSizeInBytes = 32; // 256 bits of entropy
    private const int TokenLength = 43;      // base64url length of 32 bytes, no padding

    public ManageToken Generate()
    {
        var token = Base64Url.EncodeToString(RandomNumberGenerator.GetBytes(TokenSizeInBytes));
        return new ManageToken(token, ComputeHash(token));
    }

    public bool Verify(string? token, byte[] storedHash)
    {
        if (token is null || token.Length != TokenLength)
            return false;

        return CryptographicOperations.FixedTimeEquals(ComputeHash(token), storedHash);
    }

    private static byte[] ComputeHash(string token)
        => SHA256.HashData(Encoding.UTF8.GetBytes(token));
}
