# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## Project Overview

**AlgoTradingSystem** is an enterprise-grade algorithmic trading platform for the Korean stock market, built with .NET 8.0 and C# 12.0. It implements **Clean Architecture + Domain-Driven Design (DDD)** with CQRS patterns.

**Core Purpose**: Automated trading strategy execution with real-time market data, backtesting, risk management, and hybrid UI (WPF desktop + Blazor web).

---

## Quick Reference: Common Commands

### Build and Run

```bash
# Build entire solution
dotnet build AlgoTradingSystem.sln

# Run desktop WPF application
cd src/4.Presentation/AlgoTrading.Desktop && dotnet run

# Run ASP.NET Core Web API
cd src/4.Presentation/AlgoTrading.API && dotnet run

# Run Blazor Server web application
cd src/4.Presentation/AlgoTrading.Web && dotnet run
```

### Database and Infrastructure

```bash
# Start PostgreSQL, Redis, RabbitMQ containers
cd deployment/docker && docker-compose up -d

# Apply EF Core migrations
cd src/3.Infrastructure/AlgoTrading.Infrastructure && dotnet ef database update

# Create and seed database
dotnet ef database update -v  # Verbose output for debugging
```

### Testing

```bash
# Run all tests
dotnet test AlgoTradingSystem.sln

# Run specific test project
dotnet test tests/AlgoTrading.UnitTests/AlgoTrading.UnitTests.csproj

# Run tests with code coverage
dotnet test /p:CollectCoverage=true /p:CoverletOutputFormat=opencover

# Generate coverage report
reportgenerator -reports:coverage.opencover.xml -targetdir:coveragereport

# Run single test (example)
dotnet test --filter "ClassName=AlgoTrading.Application.Tests.Handlers.CreateOrderCommandHandlerTests"
```

### User Secrets (Sensitive Configuration)

```bash
# Initialize user secrets for API project
dotnet user-secrets init --project src/4.Presentation/AlgoTrading.API

# Store sensitive values
dotnet user-secrets set "Kiwoom:Password" "your_password" --project src/4.Presentation/AlgoTrading.API
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=localhost;..." --project src/4.Presentation/AlgoTrading.API

# View all secrets
dotnet user-secrets list --project src/4.Presentation/AlgoTrading.API
```

### Development Environment Setup

```bash
# Restore all NuGet packages
dotnet restore

# Check for dependency vulnerabilities
dotnet list package --vulnerable

# Run linting/analyzers (if configured)
dotnet build --no-restore /p:EnforceCodeStyleInBuild=true
```

---

## Architecture Overview

### Layer Structure (Clean Architecture)

The codebase follows 5-layer Clean Architecture with inbound dependencies:

```
Presentation Layer (Controllers, Views, WPF/Blazor)
        ↓
Application Layer (CQRS: Commands/Queries, Handlers, Services)
        ↓
Domain Layer (Entities, Value Objects, Domain Events, Interfaces)
        ↓
Infrastructure Layer (Data Access, External Services, Caching, Message Bus)
        ↓
Shared Library (Constants, Extensions, Result Pattern)
```

**Key Principle**: Dependencies point inward. Domain layer has zero dependencies on other layers.

### Domain-Driven Design: 6 Bounded Contexts

1. **Market Data Context** (`src/1.Core/AlgoTrading.Core/Entities/MarketData/`)
   - Real-time stock prices, technical indicators, OHLC data
   - Kiwoom OpenAPI integration
   - TimescaleDB for time-series data

2. **Strategy Context** (`src/1.Core/AlgoTrading.Core/Entities/Strategy/`)
   - Trading strategies, signal generation, parameter optimization
   - Genetic algorithm, grid search, walk-forward analysis
   - ITradingStrategy interface for pluggable strategies

3. **Trading Context** (`src/1.Core/AlgoTrading.Core/Entities/Order/`)
   - Order management (create, modify, cancel)
   - Order types: Market, Limit, Conditional
   - State machine pattern for order lifecycle

4. **Account Context** (`src/1.Core/AlgoTrading.Core/Entities/Account/`)
   - Portfolio management, multi-account support
   - Position tracking (average price, P&L)
   - Asset allocation and rebalancing

5. **Risk Management Context** (`src/1.Core/AlgoTrading.Core/Entities/Risk/`)
   - VaR, CVaR calculation
   - Position limits, stop-loss, kill switch
   - Kelly Criterion position sizing

6. **Monitoring Context** (`src/4.Presentation/`)
   - Real-time dashboards and alerts
   - Email, SMS, Telegram, Slack notifications
   - System health monitoring

### CQRS Pattern

- **Commands** (Write): `src/2.Application/AlgoTrading.Application/Commands/`
  - CreateOrderCommand, StrategyActivateCommand, UpdatePortfolioCommand
  - Routed through MediatR handlers with validation/logging behaviors

- **Queries** (Read): `src/2.Application/AlgoTrading.Application/Queries/`
  - GetPortfolioQuery, GetOrderHistoryQuery, GetStrategyPerformanceQuery
  - Optimized for read performance with denormalized data models

### Key Technologies and Integration Points

| Component | Technology | Location | Purpose |
|-----------|-----------|----------|---------|
| **ORM** | Entity Framework Core 8 | `src/3.Infrastructure/Persistence/` | PostgreSQL data access |
| **Messaging** | MediatR 12.x | `src/2.Application/Handlers/` | Command/Query mediation |
| **Validation** | FluentValidation 11.x | `src/2.Application/Validators/` | Business rule validation |
| **Mapping** | AutoMapper 12.x | `src/2.Application/Mappers/` | DTO ↔ Domain entity mapping |
| **Caching** | Redis + MemoryCache | `src/3.Infrastructure/Caching/` | Multi-tier L1/L2 caching |
| **Message Queue** | RabbitMQ 3.x | `src/3.Infrastructure/MessageBus/` | Async event distribution |
| **Real-time** | SignalR | `src/4.Presentation/AlgoTrading.API/Hubs/` | Live data streaming to clients |
| **Time-series DB** | TimescaleDB (PostgreSQL extension) | Database schema | OHLC data optimization |
| **Broker API** | Kiwoom OpenAPI | `src/3.Infrastructure/ExternalServices/` | Korean stock market data |
| **Logging** | Serilog | `src/3.Infrastructure/Logging/` | Structured logging, ELK integration |

---

## Code Organization and Key Files

### Core Domain Layer

**Entities** (`src/1.Core/AlgoTrading.Core/Entities/`):
- Aggregate Roots: Order, Portfolio, Strategy, Stock
- Domain logic encapsulated in entities
- Each entity protected by invariants

**Value Objects** (`src/1.Core/AlgoTrading.Core/ValueObjects/`):
- Money, Price, Quantity, StockCode, OrderType
- Immutable, identified by value, never persist independently

**Domain Events** (`src/1.Core/AlgoTrading.Core/DomainEvents/`):
- OrderCreatedEvent, OrderFilledEvent, SignalGeneratedEvent
- Raised by entities, published through application layer
- Enable loose coupling between bounded contexts

**Interfaces** (`src/1.Core/AlgoTrading.Core/Interfaces/`):
- IOrderRepository, IStrategyRepository, IPortfolioRepository
- Implemented in Infrastructure layer
- Domain layer never references Infrastructure

### Application Layer

**Handlers** (`src/2.Application/AlgoTrading.Application/Handlers/`):
- Command handlers: Receive command → orchestrate domain operations → persist
- Query handlers: Build optimized read models → return DTOs

**Validators** (`src/2.Application/AlgoTrading.Application/Validators/`):
- FluentValidation for CQRS requests
- Run before handlers via MediatR behavior

**Mappers** (`src/2.Application/AlgoTrading.Application/Mappers/`):
- AutoMapper profiles: Domain entities → DTOs and vice versa
- Keep API contracts independent from domain model

**Behaviors** (`src/2.Application/AlgoTrading.Application/Behaviors/`):
- MediatR pipeline behaviors: ValidationBehavior, LoggingBehavior, TransactionBehavior
- Cross-cutting concerns applied to all requests

### Infrastructure Layer

**Persistence** (`src/3.Infrastructure/AlgoTrading.Infrastructure/Persistence/`):
- DbContext: Entity mappings, relationships, migrations
- Repositories: IQueryable support for specifications
- UnitOfWork: Transaction management across aggregates

**External Services** (`src/3.Infrastructure/ExternalServices/`):
- KiwoomBrokerAdapter, eBestBrokerAdapter (Adapter pattern)
- MarketDataCollector: Background service polling broker APIs
- StrategyExecutor: Background service executing signal handlers

**Caching** (`src/3.Infrastructure/Caching/`):
- L1 Cache: MemoryCache for per-process caching
- L2 Cache: Redis for distributed caching
- Cache invalidation via domain events

**Message Bus** (`src/3.Infrastructure/MessageBus/`):
- RabbitMQ configuration and connection
- Event publisher for domain events
- Event subscribers for async handlers

**Notifications** (`src/3.Infrastructure/Notifications/`):
- Email, SMS, Telegram, Slack implementations
- Called from event handlers for user alerts

---

## Important Architectural Patterns

### 1. Repository Pattern
- Repositories in Infrastructure → Interfaces in Core
- IQueryable support for Specification pattern
- Example: `IOrderRepository.Where(spec).ToListAsync()`

### 2. Specification Pattern
- DDD aggregates queried via specifications
- `PendingOrdersSpecification`, `ActiveStrategySpecification`
- Encapsulates WHERE logic, reusable across handlers/queries
- Location: `src/1.Core/AlgoTrading.Core/Specifications/`

### 3. Unit of Work Pattern
- DbContext implements IUnitOfWork
- All changes committed atomically
- Location: `src/3.Infrastructure/Persistence/`

### 4. Factory Pattern
- OrderFactory, StrategyFactory for complex object creation
- Validates invariants during construction
- Location: `src/1.Core/AlgoTrading.Core/Entities/*/Factories/`

### 5. Result Pattern (Error Handling)
- Result<T> and Result instead of throwing exceptions
- Functional error handling for validation failures
- Location: `src/1.Shared/AlgoTrading.Shared/Results/`

### 6. Domain Events
- Entities publish events via AggregateRoot.RaiseDomainEvent()
- Application layer publishes to message bus
- Event handlers process side effects asynchronously
- Prevents circular dependencies between contexts

### 7. Strategy Pattern
- ITradingStrategy interface with multiple implementations
- MovingAverageCrossStrategy, RSIBoundaryStrategy, etc.
- Enables runtime strategy selection

---

## Development Workflows

### Adding a New Feature (CQRS Example)

1. **Create Domain Logic** (Core)
   - Add/modify Aggregate Root or Entity
   - Protect with invariants in constructor/methods
   - Raise domain events if needed

2. **Create Application Logic** (Application)
   - New Command: `CreateNewFeatureCommand` + `Handler`
   - Add FluentValidation validator
   - Add AutoMapper profile if transferring data

3. **Create API Endpoint** (Presentation/API)
   - New Controller endpoint
   - Route command through MediatR
   - Return DTO

4. **Add Tests**
   - Unit test: Domain logic in isolation
   - Handler test: Mock repositories, verify side effects
   - Integration test: Full pipeline with real DB (test container)

### Modifying Database Schema

1. Create EF Core migration:
   ```bash
   cd src/3.Infrastructure/AlgoTrading.Infrastructure
   dotnet ef migrations add DescriptiveFeatureName
   ```

2. Review migration file for correctness

3. Apply migration:
   ```bash
   dotnet ef database update
   ```

4. Seed data if needed in DbContext.OnModelCreating()

### Adding an External Integration

1. Create adapter interface in Infrastructure: `IBrokerAdapter`
2. Create concrete implementation: `KiwoomBrokerAdapter`
3. Configure dependency injection in Program.cs
4. Add configuration to appsettings.json
5. Reference through interface in application logic (Dependency Inversion)

### Debugging Common Issues

**DbContext Tracking Issues**:
- Use `.AsNoTracking()` for read-only queries
- Ensure `SaveChangesAsync()` is called after modifications
- Check Unit of Work is disposed properly

**Cache Invalidation**:
- Domain events should trigger cache invalidation
- Check event handlers in Infrastructure/Caching
- Use cache tags for grouped invalidation

**MediatR Handler Not Found**:
- Ensure handler assembly registered: `builder.Services.AddMediatR(typeof(Program))`
- Check handler class naming: `*Handler` suffix required
- Verify handler implements `IRequestHandler<TRequest, TResponse>`

**Repository Transactions**:
- Always use `await _unitOfWork.SaveChangesAsync()` after domain operations
- Wrap critical sections in try/catch for rollback
- Check connection string for transaction isolation level

---

## Testing Strategy

### Test Structure

```
tests/
├── AlgoTrading.UnitTests/           # Domain and handler logic
│   ├── Entities/                    # Domain entity tests
│   ├── Handlers/                    # CQRS handler tests
│   └── Validators/                  # FluentValidation tests
├── AlgoTrading.IntegrationTests/    # Infrastructure and DB
│   ├── Repositories/                # Repository tests
│   ├── ExternalServices/            # API integration tests
│   └── Persistence/                 # EF Core/migrations tests
└── AlgoTrading.E2ETests/            # Full scenario tests
    └── Trading/                     # End-to-end trading workflows
```

### Test Patterns

**Unit Tests**:
- Test domain logic with minimal dependencies
- Use xUnit, NSubstitute/Moq for mocks
- No database or external services
- Location: `tests/AlgoTrading.UnitTests/`

**Integration Tests**:
- Use TestContainers for PostgreSQL, Redis, RabbitMQ
- Test repositories, handlers with real infrastructure
- Location: `tests/AlgoTrading.IntegrationTests/`

**E2E Tests**:
- Full scenario: command → domain → database → query
- Verify side effects (events, notifications)
- Location: `tests/AlgoTrading.E2ETests/`

---

## Performance Considerations

### Caching Strategy
- **Market Data**: Real-time, minimal caching (< 1 second)
- **Stock Metadata**: In-memory L1 (1 hour), Redis L2 (24 hours)
- **Portfolio Snapshots**: In-memory L1 (5 minutes), Redis L2 (30 minutes)
- **Invalidation**: Domain events trigger cache clear

### Database Optimization
- **TimescaleDB**: Automatic partitioning of OHLC data by date
- **Indexes**: Stock code, creation date, strategy ID
- **Connection Pooling**: EF Core default (max 100 connections)
- **Query Optimization**: Use `.AsNoTracking()` for read-only queries

### Async/Await
- All I/O operations are async (database, API, message queue)
- Never block on async calls: avoid `.Result`, `.Wait()`
- Configure default sync context: `TaskScheduler.UnobservedTaskException` handling

### SignalR Optimization
- Hub methods for real-time updates only
- Use groups for broadcasting to specific users
- MessagePack serialization for bandwidth reduction

---

## Configuration and Environment Variables

### appsettings.json Structure

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=AlgoTradingDb;Port=5432;..."
  },
  "Kiwoom": {
    "ApiUrl": "http://localhost:21000",
    "Password": "<stored in user-secrets>"
  },
  "Redis": {
    "ConnectionString": "localhost:6379"
  },
  "RabbitMQ": {
    "HostName": "localhost",
    "UserName": "guest",
    "Password": "guest"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft": "Warning"
    }
  }
}
```

### Environment-Specific Files

- `appsettings.Development.json`: Local development
- `appsettings.Production.json`: Production deployment
- `.env` (docker-compose): Docker environment variables
- User Secrets: Sensitive values (passwords, tokens)

### Docker Compose

```bash
docker-compose -f deployment/docker/docker-compose.yml up -d
```

Provides:
- PostgreSQL 17 (port 5432)
- Redis 7 (port 6379)
- RabbitMQ 3 (port 5672, admin 15672)

---

## Key Files and Their Purposes

| File/Folder | Purpose |
|-------------|---------|
| `AlgoTradingSystem.sln` | Visual Studio solution file |
| `src/1.Core/` | Domain layer - pure business logic |
| `src/2.Application/` | CQRS handlers, DTOs, validators |
| `src/3.Infrastructure/` | Data access, external services, caching |
| `src/4.Presentation/` | WPF desktop, Blazor web, REST API |
| `src/5.Shared/` | Constants, extensions, result pattern |
| `tests/` | Unit, integration, E2E test projects |
| `docs/` | Architecture docs, DDD designs, user guides |
| `deployment/` | Docker, Kubernetes, CI/CD configs |
| `scripts/` | Database, deployment automation scripts |
| `.github/workflows/` | CI/CD pipeline (currently empty) |

---

## Common Issues and Solutions

### Issue: EF Core Migrations Not Working

**Solution**:
```bash
# Check pending migrations
cd src/3.Infrastructure/AlgoTrading.Infrastructure
dotnet ef migrations list

# Remove problematic migration
dotnet ef migrations remove

# Recreate and apply
dotnet ef migrations add FeatureName
dotnet ef database update
```

### Issue: Redis Connection Refused

**Solution**:
```bash
# Ensure Redis container running
docker-compose -f deployment/docker/docker-compose.yml up redis -d

# Test connection
redis-cli ping  # Should return PONG
```

### Issue: MediatR Handler Not Executing

**Solution**:
- Verify handler class inherits from `IRequestHandler<TRequest, TResponse>`
- Check command/query class names match handler expectations
- Ensure MediatR registered in Program.cs
- Add logging to verify handler execution path

### Issue: Database Connection Timeout

**Solution**:
```bash
# Check PostgreSQL running
docker-compose -f deployment/docker/docker-compose.yml logs postgres

# Verify connection string in appsettings.json
# Ensure TCP port 5432 open on host machine
```

---

## Security Considerations

- **User Secrets**: Never commit passwords, API keys to version control
- **Connection Strings**: Store in user-secrets or environment variables
- **EF Core Migrations**: Reviewed before applying to production
- **API Authentication**: Implement JWT tokens for API endpoints
- **Data Validation**: FluentValidation on all CQRS requests
- **Encryption**: Sensitive data (passwords) encrypted at rest

---

## Additional Resources

- **README.md**: Complete project documentation in Korean
- **docs/**: Architecture diagrams, DDD context designs, development guides
- **docs/ddd-contexts/**: Detailed 6 bounded context specifications
- **docs/database/**: PostgreSQL schema, performance tuning, Redis design
- **docs/design-patterns/**: Pattern implementations with examples
- **deployment/**: Docker, Kubernetes deployment configurations
