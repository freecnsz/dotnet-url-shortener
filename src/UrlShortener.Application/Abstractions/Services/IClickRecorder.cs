using UrlShortener.Domain.Entities;

namespace UrlShortener.Application.Abstractions.Services;

public interface IClickRecorder
{
    void Record(ClickEvent clickEvent);
}
