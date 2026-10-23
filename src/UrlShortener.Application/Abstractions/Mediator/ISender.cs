namespace UrlShortener.Application.Abstractions.Mediator;

public interface ISender
{
    Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct);
}
