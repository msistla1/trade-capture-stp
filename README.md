# Trade Capture System

A robust, enterprise-grade trade capture system built with .NET 10 using Domain-Driven Design (DDD) principles and modern architectural patterns.

## Architecture Overview

This system implements a **DDD (Domain-Driven Design)** architecture with the following layers:

### 1. **Domain Layer** (`TradeCaptureSystem.Domain`)
- Contains core business entities, value objects, and domain logic
- **Entities**: `Trade` aggregate root
- **Enums**: `TcrState`, `TcrTrigger` for state machine transitions
- **Patterns**: Result pattern for operation outcomes, Rule pattern for validation
- **Rules**: `IValidationRule`, `RequiredFieldsRule`, `TradeDateRule`

### 2. **Application Layer** (`TradeCaptureSystem.Application`)
- Implements **CQRS pattern** using MediatR
- **Commands**: `ProcessTradeCommand` with handler
- **Queries**: `GetTradeByIdQuery` with handler
- **Services**: `ITradeStateMachine`, `TradeStateMachine`, `ITradeRepository`, `IDuplicateCheckService`
- **Services**: `ITradeStateMachine`, `TradeStateMachine`, `ITradeRepository`, `IDuplicateCheckService`, `IValidationService`

### 3. **Infrastructure Layer** (`TradeCaptureSystem.Infrastructure`)
- Data persistence using **Entity Framework Core**
- **DbContext**: `TradeCaptureDbContext`
- **Repositories**: `TradeRepository`
- **File Watcher**: `TradeFileWatcherService` (Background service)
- **Services**: `DuplicateCheckService`
- **Services**: `DuplicateCheckService`, `ValidationService` (orchestrates `IValidationRule` implementations)

### 4. **Host Layer** (`TradeCaptureSystem.Host`)
- Console application with dependency injection
- Configures all services and starts the file watcher

### 5. **Tests** (`TradeCaptureSystem.Tests.Unit`)
- Unit tests using xUnit, Moq, and FluentAssertions
- Tests for domain entities, rules, and command handlers

## Key Patterns Implemented

### 1. **State Machine Pattern** (Stateless)
The system uses a stateless state machine to manage trade processing workflow:

```
Received → ValidationInProgress → DuplicateCheckInProgress → ReadyForProcessing
    → Created/Updated/Rejected → Saved (or Failed with retry capability)
```

States:
- `Received`: Initial state when trade file is detected
- `ValidationInProgress`: Validates required fields and business rules
- `DuplicateCheckInProgress`: Checks if trade already exists
- `ReadyForProcessing`: Ready to create/update trade
- `Created/Updated/Rejected`: Trade processing outcome
- `Saved`: Successfully persisted (terminal state)
- `Failed`: Error occurred (can retry if retryable)

### 2. **Result Pattern**
All operations return `Result` or `Result<T>` objects indicating success/failure:
- Avoids exception-based control flow
- Provides clear error messages
- Enables functional error handling

### 3. **Rule Pattern**
Validation is decoupled using `IValidationRule` interface:
- Each rule is independently testable
- Rules can be added/removed without modifying core logic
- Examples: `RequiredFieldsRule`, `TradeDateRule`

### 4. **CQRS Pattern**
Separates read and write operations:
- **Commands**: Modify state (e.g., `ProcessTradeCommand`)
- **Queries**: Read data (e.g., `GetTradeByIdQuery`)
- Implemented using MediatR for clean separation

### 5. **Repository Pattern**
Data access is abstracted through `ITradeRepository`:
- Decouples domain from infrastructure
- Enables easy testing with mocks
- Provides a clear contract for data operations

## File Watcher Service

The system includes a background service that:
1. Monitors a configured directory for new CSV trade files
2. Automatically processes trades when files arrive
3. Archives successfully processed files
4. Moves failed files to an error directory

Configuration in `appsettings.json`:
```json
{
  "FileWatcher": {
    "WatchDirectory": "C:\\TradeFiles\\Incoming",
    "FileFilter": "*.csv",
    "FileProcessingDelayMs": 500
  }
}
```

## CSV File Format

Trade files should be in CSV format with the following structure:
```
TradeId,Counterparty,Instrument,Quantity,Price,TradeDate,SettlementDate
TRD001,Goldman Sachs,AAPL,100,150.50,2026-01-05,2026-01-07
TRD002,Morgan Stanley,MSFT,200,420.75,2026-01-05,2026-01-07
```

## Database

Uses Entity Framework Core with SQL Server LocalDB:
- Connection string: `Server=(localdb)\\mssqllocaldb;Database=TradeCaptureDb;...`
- Database is automatically created on first run
- Migrations can be added using EF Core CLI tools

## Running the Application

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB (comes with Visual Studio)

### Steps
1. Navigate to the solution directory:
   ```bash
   cd C:\Users\mcsis
   ```

2. Build the solution:
   ```bash
   dotnet build
   ```

3. Run tests:
   ```bash
   dotnet test TradeCaptureSystem.Tests.Unit/TradeCaptureSystem.Tests.Unit.csproj
   ```

4. Run the application:
   ```bash
   dotnet run --project TradeCaptureSystem.Host/TradeCaptureSystem.Host.csproj
   ```

5. Place CSV files in the watch directory:
   ```
   C:\TradeFiles\Incoming
   ```

## Testing

The solution includes comprehensive unit tests:
- **Domain Tests**: Entity behavior and validation
- **Rule Tests**: Validation rule logic with various scenarios
- **Command Handler Tests**: CQRS command processing

Run all tests:
```bash
dotnet test
```

Current test coverage: **15 tests, all passing** (ran locally during refactor)

Validation service:
- `IValidationService` was introduced to encapsulate orchestration of `IValidationRule` implementations.
- `ValidationService` lives in the `Infrastructure` layer and is registered in the host DI container:

```csharp
builder.Services.AddScoped<IValidationService, ValidationService>();
```

This decouples rule execution from the `TradeStateMachine` (which now depends on `IValidationService`), making validation easier to test and evolve.

## Project Structure

```
TradeCaptureSystem/
├── TradeCaptureSystem.Domain/           # Core domain logic
│   ├── Entities/                        # Aggregate roots
│   ├── Enums/                           # Domain enums
│   ├── Rules/                           # Validation rules
│   └── Common/                          # Shared patterns (Result)
├── TradeCaptureSystem.Application/      # Use cases and orchestration
│   ├── Commands/                        # CQRS commands
│   ├── Queries/                         # CQRS queries
│   └── Services/                        # Application services
├── TradeCaptureSystem.Infrastructure/   # External concerns
│   ├── Persistence/                     # EF Core DbContext
│   ├── Repositories/                    # Repository implementations
│   ├── Services/                        # Infrastructure services
│   └── FileWatcher/                     # File monitoring
├── TradeCaptureSystem.Host/             # Entry point
└── TradeCaptureSystem.Tests.Unit/       # Unit tests
```

## Key Technologies

- **.NET 10**: Latest .NET framework
- **Entity Framework Core 10**: ORM and database access
- **Stateless**: State machine library
- **MediatR**: CQRS implementation
- **xUnit**: Testing framework
- **Moq**: Mocking framework
- **FluentAssertions**: Fluent test assertions

## Design Principles

This system follows:
- **SOLID Principles**: Single responsibility, dependency inversion, etc.
- **Clean Architecture**: Clear separation of concerns
- **DDD**: Domain-driven design with aggregates and entities
- **Testability**: All components are fully testable and decoupled
- **Immutability**: Domain objects protect their invariants
- **Fail-Safe**: Comprehensive error handling and retry logic

## Future Enhancements

Potential improvements:
- Add integration tests
- Implement domain events for better decoupling
- Add API layer (REST or gRPC)
- Implement distributed tracing
- Add monitoring and metrics
- Support for multiple file formats (JSON, XML)
- Implement saga pattern for distributed transactions
 
## Refactor Notes

- **New orchestration services**: Introduced `IValidationService` / `ValidationService` to run `IValidationRule` implementations and centralize validation logic.
- **Persistence abstraction**: Added `ITradePersistenceService` / `TradePersistenceService` to encapsulate DB save logic (moved persistence out of `TradeStateMachine`).
- **State machine factory**: Added `ITradeStateMachineFactory` / `TradeStateMachineFactory` so handlers request a configured `ITradeStateMachine` instead of constructing it directly.
- **Repository optimization**: `ITradeRepository` now exposes `ExistsByTradeIdAsync` and `TradeRepository` implements it; `DuplicateCheckService` uses it for efficient existence checks.
- **DI registrations**: New services are registered in `TradeCaptureSystem.Host/Program.cs`:

```csharp
builder.Services.AddScoped<IValidationService, ValidationService>();
builder.Services.AddScoped<ITradePersistenceService, TradePersistenceService>();
builder.Services.AddScoped<ITradeStateMachineFactory, TradeStateMachineFactory>();
```

- **Handler change**: `ProcessTradeCommandHandler` now depends on `ITradeStateMachineFactory` and creates the state machine via the factory. This reduces responsibility and improves testability.
- **Tests updated**: Unit tests were adjusted to mock `IValidationService` and to use the `TradeStateMachineFactory` in handler tests.

These changes improve single-responsibility, dependency inversion, and testability while keeping the existing rule-based validation and state-machine workflow.
