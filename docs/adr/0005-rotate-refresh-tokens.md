# ADR 0005: Rotate refresh tokens and revoke reused families

- Status: accepted
- Date: 2026-11-10

## Context

Long-lived bearer tokens remain useful after theft unless they can be revoked. Storing a raw
refresh token would also turn a database leak into an immediately usable credential leak.

## Decision

Issue a short-lived access token and a random refresh token. Store only a SHA-256 hash of the
refresh token. On refresh, revoke it and issue a replacement in the same family. If a revoked
token is presented again, revoke the entire family.

## Consequences

- A refresh token is one-use and cannot be recovered from the database.
- Reuse detection invalidates the attacker's and legitimate client's remaining family tokens.
- Drivers must refresh more often than with a multi-day access token.
