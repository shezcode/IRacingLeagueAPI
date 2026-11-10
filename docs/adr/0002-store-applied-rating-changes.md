# ADR 0002: Store applied rating changes

- Status: accepted
- Date: 2026-11-10

## Context

Safety Rating is clamped to a bounded range. Recomputing an inverse formula after a correction
cannot recover the amount discarded at a bound, so reversing a result can incorrectly change a
driver's rating.

## Decision

Persist each result's iRating and Safety Rating before values and the actual changes applied. To
recalculate history, start from the last stored values before the affected race and replay forward.

## Consequences

- Clamping effects are recorded exactly.
- Corrections and deletions use the same replay path as new classifications.
- Stored rating deltas are derived data and are rewritten during replay.
