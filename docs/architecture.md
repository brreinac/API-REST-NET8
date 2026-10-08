# Architecture

## Overview

The solution follows Clean Architecture with a dependency direction toward the Domain.

```text
                    +----------------------+
                    |       API (.NET 8)   |
                    | Controllers/Middleware
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |    Application       |
                    | CQRS / Use Cases      |
                    +----------+-----------+
                               |
                               v
                    +----------------------+
                    |       Domain         |
                    | Entities / Rules     |
                    +----------------------+
                               ^
                               |
                    +----------+-----------+
                    |   Infrastructure     |
                    | EF Core / SQLite     |
                    +----------------------+

                    +----------------------+
                    |   Worker Service     |
                    | BackgroundService    |
                    +----------+-----------+
                               |
                               v
                         Application
```

## Responsibilities

### Domain

Contains business concepts and rules:

- `Order`
- `OrderStatus`
- `OrderPriority`
- Domain exceptions

The Domain does not reference ASP.NET Core, EF Core, SQLite, Serilog or any external framework.

### Application

Contains use cases and application contracts:

- `CreateOrderCommand`
- `GetOrderQuery`
- `GetOrdersQuery`
- Command/query handlers
- Repository and Unit of Work abstractions
- Order processing service
- DTOs and pagination model

The Application depends only on Domain and framework abstractions required for dependency injection/logging.

### Infrastructure

Contains technical implementations:

- Entity Framework Core
- SQLite
- `ApplicationDbContext`
- `OrderRepository`
- `EfUnitOfWork`

Infrastructure implements interfaces defined by Application.

### API

Contains HTTP concerns only:

- REST controllers
- Request/response contracts
- Global exception middleware
- Swagger/OpenAPI
- Serilog configuration

The API does not access EF Core directly.

### Worker

Contains the independent asynchronous process:

1. Poll pending orders.
2. Invoke the Application processing use case.
3. Simulate processing.
4. Mark the order as processed.
5. Persist the change.
6. Log the processing result.

The Worker creates a dependency injection scope for each polling cycle so scoped repositories and DbContexts are not held for the lifetime of the host.

## SOLID

- **Single Responsibility:** Controllers, handlers, repositories, middleware and worker orchestration have separate responsibilities.
- **Open/Closed:** Infrastructure implementations can be replaced behind Application interfaces.
- **Liskov Substitution:** Repository and Unit of Work implementations satisfy their abstractions.
- **Interface Segregation:** Small focused interfaces are used instead of a single service interface containing unrelated operations.
- **Dependency Inversion:** Application depends on `IOrderRepository`, `IUnitOfWork` and `IOrderProcessingService`, not on concrete infrastructure classes.

## CQRS

Commands mutate state:

- `CreateOrderCommand`

Queries read state:

- `GetOrderQuery`
- `GetOrdersQuery`

The separation is intentionally lightweight and does not introduce MediatR because the exercise does not require a mediator library.

## Persistence decision

SQLite + Entity Framework Core was selected because the technical guide recommends SQLite to simplify execution and the exercise does not require a production-scale relational database.

The repository includes a complete SQLite DDL script at `database/schema.sql`.

The application uses EF Core `EnsureCreated` at startup rather than EF migrations. This is intentional for a small technical exercise where the DDL script is also an explicit deliverable. For a production system, versioned EF Core migrations would be preferred.

## Asynchronous processing

The API never changes a newly created order to `Processed`. The Domain factory creates it as `Pending`.

The independent Worker later calls the Application processing service, which changes the state to `Processed` and records `ProcessedAt`.

Only one Worker instance should be used for this exercise. If horizontal scaling were required in a production environment, a distributed work-claiming mechanism or message broker would be introduced.
