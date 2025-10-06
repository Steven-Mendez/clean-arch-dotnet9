# Clean Backend API

A minimal API built on .NET 9 that follows a clean architecture layout for managing users, roles, and authentication. The service exposes JWT-protected endpoints for registration, login, refresh tokens, and administrative user management.

## Highlights
- Layered architecture (Domain, Application, Infrastructure, Api) with clear responsibilities and testability.
- CQRS-style command/query handling via [Cortex.Mediator](https://github.com/cortex-net/Cortex.Extensions/tree/main/src/Cortex.Mediator).
- FluentValidation, Mapster, and Hellang ProblemDetails for input validation, mapping, and consistent error responses.
- PostgreSQL persistence with Entity Framework Core and a unit-of-work abstraction.
- Serilog JSON console logging, health checks, and Swagger UI in development environments.

## Project Layout
- `src/Domain` – Entity models and business rules.
- `src/Application` – Use cases, DTOs, validators, and mediator behaviors.
- `src/Infrastructure` – EF Core context, repositories, JWT/password services, background seeders.
- `src/Api` – Minimal API setup, dependency injection, and HTTP endpoints.

## Prerequisites
- [.NET SDK 9.0](https://dotnet.microsoft.com/download)
- PostgreSQL 16+ (local instance or Docker)
- [Docker Compose](https://docs.docker.com/compose/) (optional, for containerised setup)

## Quick Start

### Run with Docker
```bash
docker compose up --build
```
This starts a PostgreSQL instance and the API (exposed on `http://localhost:5000`).

Copy `.env.example` to `.env` to override connection strings, JWT settings, or CORS origins without modifying the compose file:
```bash
cp .env.example .env
```
Update the values as needed and rerun `docker compose up --build` whenever dependencies or environment settings change.

### Run locally without Docker
1. Create a PostgreSQL database and user matching `ConnectionStrings:Postgres` in `src/Api/appsettings.Development.json`, or override via environment variables.
2. Apply migrations:
   ```bash
   dotnet ef database update --project src/Infrastructure/Infrastructure.csproj --startup-project src/Api/Api.csproj
   ```
3. Launch the API:
   ```bash
   dotnet run --project src/Api/Api.csproj
   ```
4. Navigate to `https://localhost:5001/swagger` (development default) for interactive API documentation.

## Configuration
Key settings live in `src/Api/appsettings*.json` and can be overridden via environment variables.

| Setting | Purpose |
| --- | --- |
| `ConnectionStrings:Postgres` | Database connection string used by EF Core. |
| `Jwt:*` | JWT issuer, audience, signing key, and token lifetimes. Ensure the key is at least 32 characters. |
| `Cors:AllowedOrigins` | Whitelisted origins for CORS-enabled requests. |

A startup hosted service seeds two roles (`User`, `Admin`) on boot. Update `RoleSeederHostedService` if you need additional defaults.

## Development Notes
- Pipeline behaviors add validation, logging, performance measurements, and transactional consistency around each command/query.
- The unit of work automatically updates `UpdatedAtUtc` timestamps on `User` entities before save.
- Refresh tokens are rotated on each refresh request; logout revokes tokens immediately.

## Troubleshooting
- Ensure the PostgreSQL instance is reachable and the connection string matches your environment. `docker compose logs db` is helpful when using containers.
- Validation errors are returned using the RFC7807 Problem Details format with helpful field-level messages.
- For JWT issues, confirm the signing key and audience/issuer values align across client and server.

## Next Steps
- Add CI/CD workflows to build, test, and publish container images.
- Extend the API with domain-specific dashboards or metrics once frontend requirements are ready.
