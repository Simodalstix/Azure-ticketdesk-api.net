# TicketDesk API

A minimal ASP.NET Core 8 API for managing support tickets with full observability.

## Features

- **CRUD Operations**: Full ticket and comment management
- **Validation**: FluentValidation with detailed error responses
- **Database**: PostgreSQL with EF Core migrations
- **Observability**: Health checks, Prometheus metrics, structured logging
- **API Documentation**: Swagger/OpenAPI with examples
- **Testing**: Unit tests + integration tests with Testcontainers
- **CI/CD**: GitHub Actions with Docker deployment to Azure

## Quick Start

### Prerequisites
- .NET 8 SDK
- PostgreSQL (or Docker)

### Run Locally
```bash
# Clone and restore
git clone <repo-url>
cd Azure-ticketdesk-api.net
dotnet restore

# Start PostgreSQL (Docker)
docker run --name postgres -e POSTGRES_PASSWORD=postgres -p 5432:5432 -d postgres:15

# Run migrations and start API
cd src/TicketDesk.Api
dotnet ef database update
dotnet run

# API available at: https://localhost:7000
# Swagger UI: https://localhost:7000/swagger
```

### Docker
```bash
docker build -t ticketdesk-api -f src/TicketDesk.Api/Dockerfile .
docker run -p 8080:8080 ticketdesk-api
```

## API Endpoints

### Tickets
- `GET /api/tickets` - List tickets (paginated)
- `GET /api/tickets/{id}` - Get ticket by ID
- `POST /api/tickets` - Create ticket
- `PUT /api/tickets/{id}` - Update ticket
- `DELETE /api/tickets/{id}` - Delete ticket

### Comments
- `GET /api/comments/ticket/{ticketId}` - List comments for ticket
- `POST /api/comments/ticket/{ticketId}` - Add comment to ticket

### Observability
- `GET /healthz` - Health check
- `GET /metrics` - Prometheus metrics

## Testing
```bash
# Unit tests
dotnet test tests/TicketDesk.Tests/Unit/

# Integration tests (requires Docker)
dotnet test tests/TicketDesk.Tests/Integration/

# All tests
dotnet test
```

## Deployment

### Azure App Service
1. Create Azure App Service (Linux container)
2. Set environment variables:
   - `ConnectionStrings__DefaultConnection`: PostgreSQL connection string
3. Configure GitHub secrets:
   - `AZURE_APP_NAME`: Your App Service name
   - `AZURE_PUBLISH_PROFILE`: Download from Azure portal
4. Push to main branch - automatic deployment via GitHub Actions

### Environment Variables
- `ConnectionStrings__DefaultConnection`: Database connection string
- `ASPNETCORE_ENVIRONMENT`: Environment (Development/Production)

## Architecture

```
src/TicketDesk.Api/
├── Controllers/     # API controllers
├── Models/         # Entities and DTOs
├── Data/           # EF Core DbContext
├── Validators/     # FluentValidation rules
├── Migrations/     # EF Core migrations
└── Program.cs      # Application entry point
```

## Technology Stack

- **Framework**: ASP.NET Core 8
- **Database**: PostgreSQL + Entity Framework Core
- **Validation**: FluentValidation
- **Logging**: Serilog (structured JSON)
- **Metrics**: prometheus-net
- **Testing**: xUnit + Testcontainers
- **Documentation**: Swagger/OpenAPI
- **Deployment**: Docker + Azure App Service