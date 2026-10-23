FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Restore in its own layer so it is cached until a project or package version changes
COPY Directory.Packages.props ./
COPY src/UrlShortener.Api/*.csproj src/UrlShortener.Api/
COPY src/UrlShortener.Application/*.csproj src/UrlShortener.Application/
COPY src/UrlShortener.Domain/*.csproj src/UrlShortener.Domain/
COPY src/UrlShortener.Infrastructure/*.csproj src/UrlShortener.Infrastructure/
RUN dotnet restore src/UrlShortener.Api/UrlShortener.Api.csproj

COPY src/ src/
RUN dotnet publish src/UrlShortener.Api/UrlShortener.Api.csproj -c Release -o /app --no-restore /p:UseAppHost=false

FROM build AS migrator
# Keep in sync with the Microsoft.EntityFrameworkCore version in Directory.Packages.props
RUN dotnet tool install --global dotnet-ef --version 10.0.10
ENV PATH="$PATH:/root/.dotnet/tools"
ENTRYPOINT ["dotnet-ef", "database", "update", "--project", "src/UrlShortener.Infrastructure", "--startup-project", "src/UrlShortener.Api"]

FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# curl for the healthcheck; tzdata so IANA time zone lookups never depend on the base image
RUN apt-get update \
    && apt-get install -y --no-install-recommends curl tzdata \
    && rm -rf /var/lib/apt/lists/*

COPY --from=build /app .

# Non-root user that ships with the .NET images
USER $APP_UID

ENV ASPNETCORE_HTTP_PORTS=5000
EXPOSE 5000

HEALTHCHECK --interval=30s --timeout=5s --start-period=20s --retries=3 \
    CMD curl -f http://localhost:5000/health || exit 1

ENTRYPOINT ["dotnet", "UrlShortener.Api.dll"]
