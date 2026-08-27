# LegalDocsPro

[![Build and Test](https://github.com/menarescsc/LegalDocsPro/actions/workflows/ci.yml/badge.svg)](https://github.com/menarescsc/LegalDocsPro/actions/workflows/ci.yml)
[![.NET](https://img.shields.io/badge/.NET-10.0-purple)](https://dotnet.microsoft.com/)
[![License: MIT](https://img.shields.io/badge/License-MIT-yellow.svg)](LICENSE)

LegalDocsPro is an ASP.NET Core API for managing contracts, users, and contract documents. Built with Clean Architecture principles, CQRS pattern using MediatR, and Entity Framework Core with SQL Server.

## Features

- **Contract Management**: Create, review, approve, and activate contracts with document attachments
- **User Authentication**: JWT-based authentication with role-based authorization
- **Document Storage**: Secure file upload and authorized download system
- **Audit Trail**: Automatic tracking of creation and modification timestamps
- **Pagination**: Efficient pagination and search for contract listings

## Quick Start

### Prerequisites

- [.NET SDK 10.0](https://dotnet.microsoft.com/download/dotnet/10.0)
- [SQL Server](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) or [SQL Server Express](https://www.microsoft.com/en-us/sql-server/sql-server-downloads) (or Docker)

### Option 1: Using Docker (Recommended)

```bash
# Clone the repository
git clone https://github.com/menarescsc/LegalDocsPro.git
cd LegalDocsPro

# Copy the example files
cp docker-compose.example.yml docker-compose.yml
cp .env.example .env

# Edit .env with your secrets (never commit this file)
# Then start the containers
docker compose up -d
```

The API will be available at `http://localhost:8080` with Swagger UI at `http://localhost:8080/swagger`.

### Option 2: Local Development

```bash
# Clone the repository
git clone https://github.com/menarescsc/LegalDocsPro.git
cd LegalDocsPro

# Configure secrets (User Secrets for development)
cd LegalDocsPro.Api
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=.\\SQLEXPRESS;Database=LegalDocsProDB;Trusted_Connection=True;TrustServerCertificate=True;"
dotnet user-secrets set "JwtSettings:Secret" "Your-Super-Secret-JWT-Key-At-Least-32-Characters-Long!"

# Run the API
dotnet run
```

### Option 3: Environment Variables (Production)

```bash
export ConnectionStrings__DefaultConnection="Server=your-server;Database=LegalDocsProDB;User Id=your-user;Password=your-password;"
export JwtSettings__Secret="Your-Production-Secret-Key-At-Least-32-Characters-Long!"
```

> **Important**: The `appsettings.json` file contains `CHANGE_ME` placeholders. The application will refuse to start with placeholder values.

## Build and Test

```bash
# Restore dependencies
dotnet restore LegalDocsPro.slnx

# Build the solution
dotnet build LegalDocsPro.slnx

# Run all tests
dotnet test tests/LegalDocsPro.Domain.Tests/LegalDocsPro.Domain.Tests.csproj

# Run tests with coverage
dotnet test tests/LegalDocsPro.Domain.Tests/LegalDocsPro.Domain.Tests.csproj \
  --collect:"XPlat Code Coverage" \
  --logger "trx;LogFileName=domain-tests.trx"
```

## API Endpoints

### Authentication

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/auth/register` | Register a new user |
| `POST` | `/api/auth/login` | Login and get JWT token |

### Contracts

| Method | Endpoint | Description |
|--------|----------|-------------|
| `GET` | `/api/contracts` | List contracts (paginated) |
| `GET` | `/api/contracts/{id}` | Get contract by ID |
| `POST` | `/api/contracts` | Create a new contract |
| `POST` | `/api/contracts/{id}/send-to-review` | Send contract to review |
| `POST` | `/api/contracts/{id}/approve` | Approve contract |
| `POST` | `/api/contracts/{id}/activate` | Activate contract |
| `POST` | `/api/contracts/{id}/documents` | Attach document to contract |

### Files

| Method | Endpoint | Description |
|--------|----------|-------------|
| `POST` | `/api/files/upload` | Upload a file |
| `GET` | `/api/files/download/{storedName}` | Download a file (authorized) |

> **Note**: All contract and file endpoints require authentication. Include the JWT token in the `Authorization` header as `Bearer <token>`.

## Architecture

The project follows Clean Architecture with four layers:

| Layer | Responsibility | Current Implementation |
|-------|----------------|------------------------|
| `LegalDocsPro.Domain` | Entities, status values, and domain rules | `Contract`, `User`, `Role`, and contract state transitions |
| `LegalDocsPro.Application` | Use cases and application orchestration | MediatR commands/queries and FluentValidation validators |
| `LegalDocsPro.Infrastructure` | Persistence and external technical services | Entity Framework Core, SQL Server repositories, and JWT token generation |
| `LegalDocsPro.Api` | HTTP entry point | ASP.NET Core controllers, authentication, Swagger, CORS, and file serving |

### Contract State Machine

```
Draft → InReview → Approved → Active
```

## Testing

The test suite uses **xUnit** and **FluentAssertions**:

- `LegalDocsPro.Domain.Tests`: Contract state rules and User entity behavior
- `LegalDocsPro.Application.Tests`: Command/query handlers and authorization
- `LegalDocsPro.Api.Tests`: Controllers and middleware
- `LegalDocsPro.Application.Tests`: Storage service tests

## Continuous Integration

GitHub Actions CI runs on pushes and pull requests to `main` or `develop`:

1. Builds the solution
2. Runs tests in parallel using a matrix strategy
3. Uploads test results and Cobertura coverage artifacts

## Project Structure

```
LegalDocsPro/
├── LegalDocsPro.Api/              # HTTP entry point (controllers, middleware)
├── LegalDocsPro.Application/      # Use cases (commands, queries, validators)
├── LegalDocsPro.Domain/           # Core business logic (entities, interfaces)
├── LegalDocsPro.Infrastructure/   # External concerns (EF Core, JWT, storage)
├── tests/                         # Test projects
│   ├── LegalDocsPro.Api.Tests/
│   ├── LegalDocsPro.Application.Tests/
│   └── LegalDocsPro.Domain.Tests/
├── .github/workflows/ci.yml      # CI pipeline
├── docker-compose.example.yml     # Docker template
├── .env.example                   # Environment variables template
└── README.md                      # This file
```

## Contributing

Contributions are welcome! Please follow these steps:

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'feat: add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

### Commit Convention

This project uses [Conventional Commits](https://www.conventionalcommits.org/):

- `feat:` for new features
- `fix:` for bug fixes
- `docs:` for documentation changes
- `refactor:` for code refactoring
- `test:` for adding tests
- `chore:` for maintenance tasks

## Current Limitations

- Only the Domain test layer exists; coverage percentages are not asserted or published as a quality gate
- The API depends on SQL Server and its configured connection string at runtime
- Production secret management is not implemented in this delivery
- Deployment, environment promotion, and release automation are not implemented

## Roadmap

1. Add focused Application tests for commands, queries, validation, and authorization behavior
2. Add Infrastructure integration tests against an isolated SQL Server test database
3. Add API tests for authentication, contract endpoints, file upload behavior, and error responses
4. Establish coverage thresholds after the additional test layers provide meaningful coverage data
5. Externalize production configuration and define a reviewed deployment strategy

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgments

- Built with [ASP.NET Core 10.0](https://docs.microsoft.com/en-us/aspnet/core/)
- Uses [MediatR](https://github.com/jbogard/MediatR) for CQRS pattern
- Uses [FluentValidation](https://fluentvalidation.net/) for input validation
- Uses [Entity Framework Core](https://docs.microsoft.com/en-us/ef/core/) for data access
