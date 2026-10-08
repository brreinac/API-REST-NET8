# ADR 0001 — SQLite and EnsureCreated

## Status

Accepted

## Context

The technical exercise explicitly recommends SQLite to simplify local execution and requires a DDL script in the repository.

## Decision

Use:

- SQLite
- Entity Framework Core
- `EnsureCreated()` at application/worker startup
- `database/schema.sql` as the complete DDL deliverable

## Consequences

### Positive

- No SQL Server installation is required.
- The evaluator can execute the solution with only the .NET 8 SDK.
- The database is created automatically.
- The required DDL remains available for inspection.

### Trade-off

`EnsureCreated()` is not a replacement for production schema versioning. If this solution evolved into a production service, EF Core migrations should replace `EnsureCreated()` and be executed as a controlled deployment step.
