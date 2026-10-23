using UrlShortener.Application.Abstractions.Mediator;
using UrlShortener.Application.Common.Results;

namespace UrlShortener.Application.Features.ShortUrls.DeleteLink;

public sealed record DeleteLinkCommand(string ShortCode, string? ManageToken) : IRequest<Result>;
