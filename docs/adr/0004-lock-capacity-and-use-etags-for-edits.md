# ADR 0004: Lock capacity checks and use ETags for edits

- Status: accepted
- Date: 2026-11-10

## Context

Concurrent membership joins can exceed the league's driver limit if they both check capacity
before either inserts. League and race edits have a different conflict pattern: clients read a
resource, make changes, and may later overwrite a newer edit.

## Decision

Lock the league row inside the join transaction before checking membership count. Map PostgreSQL
`xmin` to a row-version token for leagues and races, return it as an ETag, and accept optional
`If-Match` on updates.

## Consequences

- Join capacity is serialized per league while unrelated leagues remain independent.
- A client can opt into stale-edit detection without extra setup in Swagger.
- A database concurrency conflict is returned as HTTP 412.
