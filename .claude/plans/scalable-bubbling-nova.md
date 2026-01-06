# Trade Allocation and Compliance System - Implementation Plan

## Executive Summary

This plan outlines the implementation of a microservices-based trade allocation and compliance system for an asset management firm. The system will handle trade booking with multi-fund allocation, real-time compliance checking, and sophisticated allocation methodologies.

## Technology Stack

- **Platform**: .NET 8 (upgrade from existing .NET 5)
- **Database**: SQL Server with Entity Framework Core 8
- **Communication**: Synchronous REST APIs with Polly resilience patterns
- **Caching**: Memory Cache + Redis (distributed)
- **Rule Engine**: Database-driven with Roslyn C# script evaluation
- **Validation**: FluentValidation
- **Logging**: Serilog + Application Insights
- **API Documentation**: Swagger/OpenAPI

## Architecture Overview

### Microservices Structure

1. **TradeOrchestrator.API** (Port 5000)
   - Orchestrates Compliance → Allocation → Booking workflow
   - Manages state and user interactions for partial failures
   - No business logic - pure orchestration layer

2. **Compliance.API** (Port 5001)
   - Evaluates fund-level compliance rules in parallel
   - Calculates available capacity per fund
   - Rule engine with database-driven configuration

3. **Allocation.API** (Port 5002)
   - Implements pro-rata, targeted, and custom allocation strategies
   - Conflict resolution when capacity is limited

4. **TradeBooking.API** (Port 5003)
   - Persists trades and allocations with ACID guarantees
   - Generates confirmation numbers and audit trail

5. **FundData.API** (Port 5004)
   - Master data for funds, strategies, and compliance rules
   - CRUD operations for fund configuration

6. **ReferenceData.API** (Port 5005)
   - Securities, counterparties, ratings data
   - Shared reference data across all services

## Workflow

### Standard Flow (All Funds Pass Compliance)

```
UI → TradeOrchestrator → Compliance (parallel evaluation across funds)
                       ↓
                    Allocation (distribute based on methodology)
                       ↓
                    TradeBooking (persist with transaction)
                       ↓
                    Response to UI (trade confirmed)
```

### Partial Failure Flow (Some Funds Fail)

```
UI → TradeOrchestrator → Compliance (2 of 5 funds fail)
                       ↓
                    Response: AwaitingApproval status
                       ↓
UI displays failed funds ← User Decision:
                           - Proceed with 3 passing funds
                           - Cancel entire trade
                           - Modify and resubmit
                       ↓
User approves partial → Allocation (only compliant funds)
                       ↓
                    TradeBooking
                       ↓
                    Response to UI
```

## Domain Model

### Core Entities

**Trade Aggregate**
- Trade (root)
  - Id: Guid
  - SecurityId: string
  - CounterpartyId: string
  - TotalAmount: Money (value object)
  - Price: Price (value object)
  - TradeDate: TradeDate (value object)
  - Status: enum
  - Allocations: List\<TradeAllocation\>

**Fund Aggregate**
- Fund (root)
  - Id: string
  - Name: string
  - StrategyId: string
  - NetAssetValue: Money
  - Status: enum
  - ComplianceRules: List\<ComplianceRule\>

**ComplianceRule Entity**
- Id: Guid
- FundId: string
- RuleType: enum (PercentageLimit, CategoricalRestriction, etc.)
- Configuration: JSON column with expression and parameters
- Priority: int
- EffectiveDate/ExpiryDate: dates

### Value Objects

- Money(Amount: decimal, Currency: string)
- Price(Value: decimal, Currency: string)
- TradeDate(Date: DateTime)
- Percentage(Value: decimal)

## Database Schema

### Key Tables

**Trades**
- Primary key: Id (uniqueidentifier)
- Foreign data: SecurityId, CounterpartyId
- Financial: TotalAmount, Price, Currency
- Audit: CreatedBy, CreatedAt, ApprovedBy, ApprovedAt
- Indexes: TradeDate, SecurityId, Status

**TradeAllocations**
- Primary key: Id (uniqueidentifier)
- Foreign keys: TradeId → Trades
- Fund reference: FundId
- Allocation: AllocatedAmount, Shares, PercentageOfTotal
- Status: enum, ConfirmationNumber

**ComplianceRules**
- Primary key: Id (uniqueidentifier)
- FundId: string (indexed)
- Configuration: NVARCHAR(MAX) with JSON constraint
- Priority, IsActive, EffectiveDate, ExpiryDate
- Indexes: FundId, Active+Effective dates, RuleType

**Funds, Securities, Counterparties, PortfolioHoldings**
- Standard reference data tables with appropriate indexes

**OrchestrationStates**
- Stores workflow state for each request
- JSON columns for step results (compliance, allocation, booking)
- Enables resume/rollback capabilities

## Compliance Rule Engine

### Rule Storage Format (JSON in Configuration column)

```json
{
  "expression": "portfolio.GetIssuerExposure(security.IssuerId) + trade.Amount <= fund.NAV * 0.05",
  "parameters": {
    "maxPercentage": 5.0,
    "calculationType": "IssuerExposure"
  },
  "metadata": {
    "displayFormat": "Max {maxPercentage}% exposure to single issuer",
    "errorMessage": "Exceeds maximum issuer exposure of {maxPercentage}%"
  }
}
```

### Rule Types Supported

1. **PercentageLimit**: Max 5% single issuer, max 10% security position
2. **CategoricalRestriction**: Investment grade only, specific sectors
3. **SecurityTypeRestriction**: Bonds except T-Bills, equities only
4. **RatingRequirement**: Minimum credit rating (BBB+, etc.)

### Evaluation Engine

- Uses **Roslyn C# Script API** for expression evaluation
- Compiles and caches expressions for performance
- Provides context: fund, security, trade, portfolio, parameters
- Calculates capacity as minimum across all rules
- Returns pass/fail + capacity for each rule

### Capacity Calculation

For each fund:
1. Retrieve all active rules (ordered by priority)
2. Evaluate each rule in parallel
3. Calculate capacity for passing rules
4. **Available capacity = MIN(all rule capacities)**
5. If any rule fails → capacity = 0

## Allocation Strategies

### 1. Pro-Rata Allocation

- Calculate total capacity across all funds
- Allocate proportionally: `fund.allocation = (fund.capacity / total.capacity) * trade.amount`
- Handle rounding by allocating remainder to fund with most room

### 2. Targeted Allocation

- User specifies target amount per fund
- Process funds in priority order
- Cap allocation at min(target, capacity, remaining amount)
- Stop when trade fully allocated or no more capacity

### 3. Custom Allocation

- User specifies weights per fund
- First pass: allocate by weight with capacity constraints
- Second pass: redistribute shortfall to funds with remaining capacity (by priority)
- Return warnings for reallocations

### Conflict Resolution

- All strategies respect capacity limits (compliance-driven)
- Priority field determines tie-breaking and reallocation order
- Partial allocation warnings returned in response

## API Contracts

### TradeOrchestrator Endpoints

**POST /api/trades**
- Request: TradeSubmission (security, funds, amount, methodology)
- Response: OrchestrationResponse (status, results from each step)
- Status codes: 200 OK, 400 Bad Request, 500 Error

**POST /api/trades/{requestId}/approve**
- Request: PartialApprovalRequest (approved fund IDs)
- Response: OrchestrationResponse (continues from allocation)

**GET /api/trades/{requestId}**
- Response: OrchestrationState (current step, results)

### Compliance Endpoints

**POST /api/compliance/evaluate**
- Request: ComplianceRequest (security, funds, trade details)
- Response: ComplianceResponse (per-fund results, overall status)

**GET /api/compliance/rules/{fundId}**
- Response: List\<ComplianceRule\> (active rules for fund)

### Allocation Endpoints

**POST /api/allocation/distribute**
- Request: AllocationRequest (funds with capacity, methodology)
- Response: AllocationResponse (per-fund allocations, summary)

### TradeBooking Endpoints

**POST /api/trades/book**
- Request: TradeBookingRequest (trade + allocations)
- Response: TradeBookingResponse (trade ID, confirmations)
- Status: 201 Created

**GET /api/trades/{id}**
- Response: Trade entity with allocations

## Error Handling

### Global Exception Handler

Maps exceptions to appropriate HTTP status codes:
- ValidationException → 400 Bad Request
- BusinessRuleException → 422 Unprocessable Entity
- NotFoundException → 404 Not Found
- RuleEvaluationException → 500 Internal Server Error
- Generic Exception → 500 with sanitized message

### Custom Exceptions

- BusinessRuleException(ruleId, message)
- NotFoundException(entityName, entityId)
- RuleEvaluationException(ruleId, innerException)

All responses include traceId for correlation.

## Performance & Scalability

### Caching Strategy

**Memory Cache (per service instance)**
- Fund data: 30 min absolute, 10 min sliding
- Compliance rules: 60 min absolute, 15 min sliding
- Reference data: 4 hours absolute, 1 hour sliding

**Distributed Cache (Redis)**
- Shared across service instances
- Critical for multi-instance deployments
- Cache invalidation on rule updates

### Database Indexing

High-priority indexes:
- Trades: (TradeDate DESC, Status), (SecurityId, TradeDate)
- TradeAllocations: (FundId, Status), (TradeId)
- ComplianceRules: (FundId, IsActive, Priority), (EffectiveDate, ExpiryDate)
- PortfolioHoldings: (FundId, AsOfDate DESC), (SecurityId, AsOfDate)

Columnstore index on ComplianceEvaluations for analytics.

### HTTP Client Resilience

Using Polly policies:
- **Retry**: 3 attempts with exponential backoff
- **Circuit Breaker**: Opens after 5 failures, 30s timeout
- **Timeout**: 10s per request, 30s overall

### Parallel Processing

- Compliance evaluates all funds in parallel (Task.WhenAll)
- Allocation calculates all fund allocations concurrently
- Trade booking uses database transaction for atomicity

## Transaction Management

### Trade Booking Transaction Scope

```
BEGIN TRANSACTION
  1. Insert Trade record
  2. Insert TradeAllocation records (multiple)
  3. Insert AuditLog entries
  4. Update OrchestrationState
COMMIT TRANSACTION
```

On failure: Rollback entire transaction, return error to orchestrator.

### Orchestration Rollback

If booking fails after compliance/allocation:
1. Call TradeBooking API to cancel/void trade (if partially committed)
2. Update OrchestrationState to Failed
3. Return error to user

Compliance and Allocation are read-only (no rollback needed).

## Implementation Steps

### Phase 1: Foundation (Week 1-2)

1. **Create Solution Structure**
   - 6 microservice projects (.NET 8)
   - Shared contracts and common libraries
   - Test projects (unit, integration, E2E)

2. **Database Setup**
   - Create databases in SQL Server
   - Implement Entity Framework Core DbContexts
   - Run migrations for core tables
   - Seed reference data (securities, counterparties)

3. **Shared Infrastructure**
   - Domain model (entities, value objects)
   - DTOs and contracts
   - Exception types
   - Middleware (global exception handler)
   - HttpClient configuration with Polly

### Phase 2: Reference Services (Week 2-3)

4. **FundData.API**
   - Controllers: Funds, Strategies, Rules (CRUD)
   - Repositories with EF Core
   - Memory cache decorator
   - Swagger documentation

5. **ReferenceData.API**
   - Controllers: Securities, Counterparties
   - Portfolio holdings repository (for compliance)
   - Caching layer

### Phase 3: Core Business Services (Week 3-5)

6. **Compliance.API**
   - Rule evaluation engine (Roslyn integration)
   - Capacity calculation algorithm
   - ComplianceService with parallel evaluation
   - Rule repository with JSON deserialization
   - Unit tests for rule engine

7. **Allocation.API**
   - Three strategy implementations (ProRata, Targeted, Custom)
   - Strategy factory pattern
   - AllocationService orchestration
   - Unit tests for each strategy

8. **TradeBooking.API**
   - TradeBookingService with transactions
   - Trade and allocation repositories
   - Confirmation number generation
   - Audit logging

### Phase 4: Orchestration (Week 5-6)

9. **TradeOrchestrator.API**
   - OrchestrationService with step-by-step workflow
   - OrchestrationState repository
   - HTTP clients to downstream services
   - Partial failure handling
   - Rollback logic

10. **Integration Testing**
    - End-to-end workflow tests
    - Partial failure scenarios
    - Error handling validation
    - Performance testing

### Phase 5: Cross-Cutting Concerns (Week 6-7)

11. **Logging & Monitoring**
    - Serilog configuration
    - Application Insights telemetry
    - Custom metrics (compliance duration, allocation counts)
    - Correlation IDs across services

12. **Validation**
    - FluentValidation validators for all DTOs
    - Request validation middleware
    - Business rule validation

13. **Caching**
    - Redis distributed cache setup
    - Cache invalidation strategy
    - Cache performance testing

### Phase 6: Testing & Documentation (Week 7-8)

14. **Testing**
    - Unit test coverage > 80%
    - Integration tests for each service
    - E2E tests for complete workflows
    - Load testing for performance validation

15. **Documentation**
    - API documentation (Swagger)
    - Architecture diagrams
    - Deployment guides
    - Rule configuration manual

### Phase 7: Deployment (Week 8)

16. **Deployment Preparation**
    - Docker containerization
    - Kubernetes manifests / Azure App Service config
    - Database migration scripts
    - Environment configuration (dev, staging, prod)
    - Health check endpoints

17. **Production Readiness**
    - Security review (authentication, authorization)
    - Performance benchmarking
    - Disaster recovery plan
    - Monitoring dashboards

## Critical Files to Create

### Service Projects

- `TradeOrchestrator.API/Program.cs` - Minimal hosting bootstrap
- `TradeOrchestrator.API/Controllers/TradeOrchestrationController.cs`
- `TradeOrchestrator.API/Services/TradeOrchestrationService.cs`
- `Compliance.API/RuleEngine/RuleEvaluator.cs` - Roslyn script engine
- `Compliance.API/RuleEngine/RoslynScriptEngine.cs`
- `Compliance.API/Services/ComplianceService.cs`
- `Allocation.API/Strategies/ProRataAllocationStrategy.cs`
- `Allocation.API/Strategies/TargetedAllocationStrategy.cs`
- `Allocation.API/Strategies/CustomAllocationStrategy.cs`
- `Allocation.API/Services/AllocationService.cs`
- `TradeBooking.API/Services/TradeBookingService.cs`

### Domain Models

- `Shared.Contracts/Entities/Trade.cs`
- `Shared.Contracts/Entities/Fund.cs`
- `Shared.Contracts/Entities/ComplianceRule.cs`
- `Shared.Contracts/ValueObjects/Money.cs`
- `Shared.Contracts/ValueObjects/Price.cs`

### DTOs

- `Shared.Contracts/DTOs/TradeSubmission.cs`
- `Shared.Contracts/DTOs/ComplianceRequest.cs`
- `Shared.Contracts/DTOs/ComplianceResponse.cs`
- `Shared.Contracts/DTOs/AllocationRequest.cs`
- `Shared.Contracts/DTOs/AllocationResponse.cs`
- `Shared.Contracts/DTOs/OrchestrationResponse.cs`

### Infrastructure

- `Shared.Infrastructure/HttpClients/ComplianceServiceClient.cs`
- `Shared.Infrastructure/HttpClients/AllocationServiceClient.cs`
- `Shared.Infrastructure/HttpClients/HttpClientConfiguration.cs` - Polly policies
- `Shared.Infrastructure/Caching/CachedRepository.cs` - Decorator pattern
- `Shared.Common/Middleware/GlobalExceptionHandler.cs`

### Database

- `Compliance.Infrastructure/Data/ComplianceDbContext.cs`
- `TradeBooking.Infrastructure/Data/TradeDbContext.cs`
- `FundData.Infrastructure/Data/FundDataDbContext.cs`
- `Migrations/*.cs` - EF Core migrations for each database

### Configuration

- `appsettings.json` for each service (connection strings, URLs, cache config)
- `appsettings.Development.json` (local dev overrides)
- `appsettings.Production.json` (production settings)

## Success Criteria

1. **Functional**
   - Trade booking completes successfully for all-passing scenario
   - Partial failure prompts user for approval
   - All three allocation strategies work correctly
   - Compliance rules evaluate accurately with capacity calculation

2. **Performance**
   - Compliance evaluation: < 500ms for 10 funds
   - Full trade booking: < 2 seconds end-to-end
   - Support 100 concurrent trade submissions

3. **Quality**
   - Unit test coverage > 80%
   - Zero critical security vulnerabilities
   - All API endpoints documented in Swagger
   - Comprehensive error handling and logging

4. **Scalability**
   - Horizontal scaling of each service
   - Distributed cache for multi-instance deployment
   - Database indexes support query performance at scale

## Design Decisions Rationale

### Why Synchronous REST over Async Messaging?

- **Predictability**: User waits for complete result, simpler UX
- **Debugging**: Easier to trace request flow
- **Consistency**: Orchestrator ensures all-or-nothing semantics
- **Simplicity**: No need for saga pattern or event sourcing initially

Future: Can add async processing for non-urgent trades or batch operations.

### Why Database-Driven Rules over Code-Based?

- **Flexibility**: Business users can modify rules without deployment
- **Auditability**: Rule changes tracked in database
- **Dynamic**: Rules can be effective-dated and expired
- **Versioning**: Historical rules preserved for compliance

Trade-off: Requires robust expression evaluation (using Roslyn).

### Why Parallel Capacity Calculation?

- **Fairness**: All funds evaluated with same portfolio state
- **Performance**: Faster than sequential processing
- **Accuracy**: Avoids order-dependency bias

Trade-off: More complex conflict resolution, but handled by allocation strategies.

## Risk Mitigation

### Risk: Rule Expression Bugs

**Mitigation**:
- Comprehensive unit tests for each rule type
- Sandbox evaluation (no file system access)
- Validation of rule syntax before saving
- Rollback capability for rule changes

### Risk: Performance Degradation at Scale

**Mitigation**:
- Caching at multiple levels
- Database indexing strategy
- Load testing before production
- Horizontal scaling capability

### Risk: Partial Failure Complexity

**Mitigation**:
- Clear UX for approval workflow
- State management in OrchestrationStates table
- Ability to resume from any step
- Comprehensive error messages

### Risk: Data Consistency

**Mitigation**:
- Database transactions for atomic operations
- Rollback logic in orchestrator
- Audit trail for all changes
- Idempotency keys for retry scenarios

## Maintenance & Operations

### Monitoring Dashboards

- Trade booking success rate
- Compliance evaluation duration per fund
- Allocation methodology usage
- API error rates and latencies
- Cache hit rates

### Alerts

- Failed trade bookings > 5% in 15 minutes
- Compliance service error rate > 1%
- Database connection pool exhaustion
- Rule evaluation timeouts

### Operational Procedures

- **Adding New Rule Type**: Update RuleDefinitionCatalog, test, deploy
- **Fund Configuration**: Use FundData API, cache invalidation automatic
- **Schema Changes**: Generate migration, test in staging, deploy during maintenance window
- **Scaling**: Add service instances, ensure Redis cache configured

## Conclusion

This architecture provides a **scalable, maintainable, and performant** solution for trade allocation and compliance. Key strengths:

- **Separation of Concerns**: Each service has single responsibility
- **Flexibility**: Database-driven rules, multiple allocation strategies
- **Resilience**: Retry policies, circuit breakers, transaction management
- **Observability**: Comprehensive logging, metrics, and tracing
- **User Experience**: Handles partial failures gracefully with approval workflow

The implementation leverages existing patterns from the catalog service (repository pattern, record types, dependency injection) while adding production-grade capabilities required for financial systems.
