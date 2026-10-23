namespace UrlShortener.Application.Abstractions.Services;

public sealed record ManageToken(string Token, byte[] Hash)
{
    // Keeps the plain token out of logs
    public override string ToString() => "ManageToken { Token = ***, Hash = *** }";
}
