# LegalDocsPro

LegalDocsPro is an ASP.NET Core API for managing contracts, users, and contract documents. The current delivery establishes the first automated test layer and a GitHub Actions CI workflow while keeping deployment out of scope.

## Quick Start

Prerequisites:

- .NET SDK 10.0
- SQL Server or SQL Server Express for running the API locally

### Secret Configuration

The API requires two secrets that MUST be configured before it will start. The application performs fail-fast validation and will refuse to start with placeholder values.

**Development (User Secrets):**

```bash
cd LegalDocsPro.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=LegalDocsProDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Secret" "Your-Super-Secret-JWT-Key-At-Least-32-Characters-Long!"
```

**Production (Environment Variables):**

```bash
export ConnectionStrings__DefaultConnection="Server=your-server;Database=LegalDocsProDB;User Id=your-user;Password=your-password;"
export JwtSettings__Secret="Your-Production-Secret-Key-At-Least-32-Characters-Long!"
```

The `appsettings.json` file contains `CHANGE_ME` placeholders. Do not commit real secrets to source control.

### Build and Test

Build and test from the repository root:

```bash
dotnet restore LegalDocsPro.slnx
dotnet build LegalDocsPro.slnx
dotnet test tests/LegalDocsPro.Domain.Tests/LegalDocsPro.Domain.Tests.csproj
```

Run the domain tests with Cobertura coverage and TRX results:

```bash
dotnet test tests/LegalDocsPro.Domain.Tests/LegalDocsPro.Domain.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --logger "trx;LogFileName=domain-tests.trx"
```

The generated test results and coverage files are written below the test project's `TestResults/` directory. Coverlet's collector produces Cobertura coverage output.

To run the API, configure the `DefaultConnection` connection string and JWT settings used by `LegalDocsPro.Api/appsettings.json`, then use:

```bash
dotnet run --project LegalDocsPro.Api/LegalDocsPro.Api.csproj
```

The API exposes Swagger and applies Entity Framework Core migrations at startup. The current database is SQL Server.

## Architecture

| Layer | Responsibility | Current implementation |
| --- | --- | --- |
| `LegalDocsPro.Domain` | Entities, status values, and domain rules | `Contract`, `User`, `Role`, and contract state transitions |
| `LegalDocsPro.Application` | Use cases and application orchestration | MediatR commands/queries and FluentValidation validators |
| `LegalDocsPro.Infrastructure` | Persistence and external technical services | Entity Framework Core, SQL Server repositories, and JWT token generation |
| `LegalDocsPro.Api` | HTTP entry point | ASP.NET Core controllers, authentication, Swagger, CORS, and file serving |

Contracts currently move through the domain states `Draft`, `InReview`, `Approved`, and `Active`. SQL Server migrations are maintained in `LegalDocsPro.Infrastructure/Migrations`.

## Testing

The current automated test project is `tests/LegalDocsPro.Domain.Tests`. It uses xUnit and FluentAssertions and covers the observable `Contract` state rules and the current `User` constructor behavior. Application, Infrastructure, API, integration, and end-to-end test projects are not implemented yet.

## Continuous Integration

`.github/workflows/ci.yml` runs on pushes and pull requests targeting `main` or `develop`. It restores and builds the solution, then runs the existing test projects through a matrix so they can execute in parallel. Each test job uploads TRX results and Cobertura coverage artifacts.

CI has no deployment stage yet. GitHub environments and deployment automation will be introduced only when deployment is designed and implemented.

## Current Limitations

- Only the Domain test layer exists; coverage percentages are not asserted or published as a quality gate.
- The API still depends on SQL Server and its configured connection string at runtime.
- Runtime configuration includes JWT and database settings in `appsettings.json`; production secret management is not implemented in this delivery.
- Deployment, environment promotion, and release automation are not implemented.

## Roadmap

1. Add focused Application tests for commands, queries, validation, and authorization behavior.
2. Add Infrastructure integration tests against an isolated SQL Server test database, including migrations and repository behavior.
3. Add API tests for authentication, contract endpoints, file upload behavior, and error responses.
4. Establish coverage thresholds after the additional test layers provide meaningful coverage data.
5. Externalize production configuration and define a reviewed deployment strategy before adding deployment workflows.
