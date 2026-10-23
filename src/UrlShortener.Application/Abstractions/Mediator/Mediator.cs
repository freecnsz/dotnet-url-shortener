using Microsoft.Extensions.DependencyInjection;

namespace UrlShortener.Application.Abstractions.Mediator;

public class Mediator(IServiceProvider provider) : ISender
{
    public async Task<TResponse> Send<TResponse>(IRequest<TResponse> request, CancellationToken ct = default)
    {
        var requestType = request.GetType();

        var handlerType = typeof(IRequestHandler<,>).MakeGenericType(requestType, typeof(TResponse));

        var handler = provider.GetRequiredService(handlerType);

        var method = handlerType.GetMethod("Handle");

        return await (Task<TResponse>)method!.Invoke(handler, [request, ct])!;
    }
}
