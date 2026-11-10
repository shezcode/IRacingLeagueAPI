# Portfolio brief: iRacing League API

## One-line summary

A .NET 10 and PostgreSQL API for sim-racing leagues. It calculates standings and project-defined
iRacing-style driver ratings from complete race classifications and keeps them correct after
corrections or deletions.

## Origin

The project began as a layered class project for the DWES web-development module. The original
domain and service work was extended into an API with PostgreSQL persistence, token-based auth,
private league invites, replayable ratings, and deployable containers.

## Technical shape

Five projects form the dependency structure: `API`, `Business`, `Data`, `Data.EF`, and `Models`.
Business uses async repository contracts and a unit of work; EF Core stays in `Data.EF`. PostgreSQL
uses row-level locks for membership capacity, `xmin` for edit concurrency, and cascades for
dependent deletes. A rating replay recalculates affected drivers from stored race outcomes.

## Features to highlight

- Time-limited, single-use private league invites with SHA-256 token storage.
- Atomic full-classification replacement, server-side points, fastest-lap bonuses, and CSV preview
  and import.
- Pairwise Elo-style iRating, Safety Rating, license, and wins derived from completed races.
- Public driver profiles separated from private email-bearing `/me` data.
- Career totals and rating history filtered by the viewer's access to private leagues.
- Refresh-token rotation and reuse revocation, structured problem documents, pagination, ETags,
  Docker Compose, and GitHub Actions CI.

## Honest scope

- The project does not implement or claim to reproduce iRacing's proprietary rating algorithm.
- Automated tests are intentionally out of scope; verification is by manual API walkthroughs.
- There is no frontend. Swagger UI is the API explorer.
- The live host and public Swagger URL are pending deployment configuration.
- Password hashes created before the framework hasher change are not migrated automatically; a new
  deployment starts with a fresh database.
