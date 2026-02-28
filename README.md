# FinanceTracker

[![CI](https://github.com/OWNER/REPO/actions/workflows/ci.yml/badge.svg)](https://github.com/OWNER/REPO/actions/workflows/ci.yml)
[![codecov](https://codecov.io/gh/OWNER/REPO/graph/badge.svg)](https://codecov.io/gh/OWNER/REPO)

A .NET 8 Web API built with Clean Architecture for personal finance tracking.

## Architecture

This solution follows Clean Architecture with the following projects:

| Project | Description |
|---------|-------------|
| **FinanceTracker.Domain** | Entities, value objects, and domain logic. No dependencies. |
| **FinanceTracker.Application** | Use cases, CQRS (MediatR), FluentValidation, AutoMapper. Depends on Domain. |
| **FinanceTracker.Infrastructure** | EF Core, Dapper, Redis, MassTransit/RabbitMQ. Implements Application abstractions. |
| **FinanceTracker.API** | ASP.NET Core Web API, Swagger, Serilog. Presentation layer. |
| **FinanceTracker.UnitTests** | xUnit unit tests. |
| **FinanceTracker.IntegrationTests** | xUnit integration tests. |

## Project References

```
API → Application, Domain, Infrastructure
Application → Domain
Infrastructure → Application
UnitTests → Application, Domain
IntegrationTests → API
```

## NuGet Packages

### Application
- MediatR – CQRS/mediator pattern
- FluentValidation – Request validation
- AutoMapper – Object mapping

### Infrastructure
- Microsoft.EntityFrameworkCore – ORM
- Dapper – Micro-ORM for queries
- StackExchange.Redis – Caching
- MassTransit.RabbitMQ – Message bus

### API
- Swashbuckle.AspNetCore – Swagger/OpenAPI
- Serilog.AspNetCore – Structured logging

## Getting Started

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)

### Build & Run

```bash
# Restore packages
dotnet restore

# Build
dotnet build

# Run API
dotnet run --project src/FinanceTracker.API/FinanceTracker.API.csproj

# Run tests
dotnet test
```

### API Documentation

When running in Development, Swagger UI is available at `https://localhost:5001/swagger` (or the port configured in `launchSettings.json`).

## Coverage Badge (Coverlet + ReportGenerator + Codecov)

Replace `OWNER` and `REPO` with your GitHub username and repository name:

```markdown
[![codecov](https://codecov.io/gh/OWNER/REPO/graph/badge.svg)](https://codecov.io/gh/OWNER/REPO)
```

For the badge to display coverage data, add your repo at [codecov.io](https://codecov.io) (free for public repos). The CI workflow uploads Coverlet Cobertura reports to Codecov automatically.

## License

MIT
