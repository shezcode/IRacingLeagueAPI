# ADR 0001: PostgreSQL and ratings derived by replay

- Status: accepted
- Date: 2026-11-10

## Context

League membership needs a row lock to enforce capacity under concurrent joins. The API also needs
native integer arrays for league points tables, relational cascades, and a stable way to serialize
rating recalculations across requests.

## Decision

Use PostgreSQL as the system of record. Store race classifications as source data and treat cached
driver ratings as derived values. When race history changes, replay completed races in schedule
order across leagues and persist the resulting driver and per-result rating state.

## Consequences

- PostgreSQL transactions, advisory locks, `xmin`, arrays, and foreign-key cascades are available to
  the persistence layer.
- A rating correction may require replaying later races, but does not depend on fragile inverse
  arithmetic.
- The database and API are run together through Docker Compose for local development.
