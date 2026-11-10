# ADR 0003: A full classification is the unit of change

- Status: accepted
- Date: 2026-11-10

## Context

Position points, fastest-lap awards, and pairwise Elo depend on the entire race field. Updating one
result at a time exposes intermediate states and makes tied or duplicated positions hard to
validate atomically.

## Decision

Accept a complete classification in one request. Validate the full field, calculate points and
fastest lap on the server, replace the prior result set in one transaction, and replay ratings
from that race. CSV import and JSON submission use the same classification service.

## Consequences

- Clients cannot select their own points or update a single result independently.
- Invalid classifications leave the stored race unchanged.
- A preview can return the calculated classification without persisting it.
