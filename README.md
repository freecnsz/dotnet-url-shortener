# URL Shortener – Backend

ASP.NET Core (.NET 10) API for an anonymous URL shortener. No accounts: creating a link returns a
manage token, and that token is the only way to view stats or delete the link later.

**Live:** https://vds.freecnsz.com

## What it does

- Short codes are 7 random alphanumeric characters
- Each new link comes with a QR code (PNG) and a manage token; only the token's SHA-256 hash is stored
- Links expire after 14 days, stats stay viewable for 6 more days, then an hourly Hangfire job soft-deletes them
- Redirects are cached in Redis. Redis is optional: if it goes down, lookups fall back to PostgreSQL
- Clicks are queued in memory and written to the database in batches, so redirects don't wait on inserts
- Stats: daily clicks (in the time zone you ask for), top referrers, countries (via Cloudflare's `CF-IPCountry`, off by default)
- Link creation is rate limited to 10 requests per minute per IP

## Stack

PostgreSQL + EF Core, Redis, Hangfire, Serilog + Seq, QRCoder. The code is split into
Domain / Application / Infrastructure / Api, with a small hand-written mediator.

## API

| Method | Path | Notes |
|---|---|---|
| `POST` | `/api/links` | Body: `{ "originalUrl": "https://..." }` |
| `GET` | `/{shortCode}` | Redirects to the original URL |
| `GET` | `/api/links/{shortCode}/stats?timeZone=Europe/Istanbul` | Needs `X-Manage-Token` header |
| `DELETE` | `/api/links/{shortCode}` | Needs `X-Manage-Token` header |
| `GET` | `/health` | Checks PostgreSQL only |

Errors are returned as Problem Details with a `code` field (e.g. `RateLimit.Exceeded`).
Sample requests are in [UrlShortener.Api.http](src/UrlShortener.Api/UrlShortener.Api.http).

## Running locally

Start the dependencies with Docker:

```bash
cp .env.example .env   # fill in the passwords
docker compose up -d postgres redis seq
```

Set the connection strings for the API (user secrets are already enabled on the project):

```bash
cd src/UrlShortener.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=urlshortener;Username=urlshortener;Password=<your password>"
dotnet user-secrets set "ConnectionStrings:RedisConnection" "localhost:6379,abortConnect=false"
dotnet user-secrets set "ShortUrlOptions:BaseUrl" "http://localhost:5248"
```

Apply migrations and run:

```bash
dotnet ef database update --project ../UrlShortener.Infrastructure
dotnet run
```

In Development the Scalar UI is at `/scalar` and the Hangfire dashboard at `/hangfire`.
Seq is at http://localhost:5341.

## Deployment

`docker-compose.yml` runs the whole thing: PostgreSQL, Redis, Seq, the API and the frontend
(nginx with Let's Encrypt certificates). It expects the frontend repo next to this one at
`../url-shortener-frontend`.

```bash
cp .env.example .env   # set BASE_URL to your real domain
docker compose --profile tools run --rm migrator
docker compose up -d --build
```

The API, database, Redis and Seq only listen on `127.0.0.1`; only the frontend is exposed publicly.

## Author

Built by **Özgür Cansız** – [freecnsz.com](https://freecnsz.com)

Feedback is welcome. If you spot a bug, have an idea, or think something could be done better,
open an issue or start a discussion.

## License

[AGPL-3.0](LICENSE). If you run a modified version as a public service, you must publish your changes.
