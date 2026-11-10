# Code Review & Roadmap to a Publishable Portfolio Project

> Archived review from 2026-11-10 at the original .NET 8 / SQL Server baseline. Its issue list is
> historical and has been implemented or superseded by [`SPEC.md`](SPEC.md) and the local ticket
> tracker. The live deployment remains outstanding.

Reviewed on 2026-11-10 at commit `7cadbe5`. The solution builds cleanly (0 warnings, 0 errors).
There are no tests, so none of the findings below have been confirmed by running the app. Each
one comes from reading the code path end to end.

This file is meant to be turned into an implementation spec. Phases are ordered by dependency:
each phase assumes the ones before it are done.

---

## 1. Current state in one paragraph

This is a clean, well-commented layered ASP.NET Core 8 API (Models / Data / Data.EF / Business /
API) with JWT auth, role, ownership and membership-based authorization, EF Core code-first on SQL
Server, and Docker Compose. Its strongest feature is the domain logic: entering, correcting or
deleting a result also updates standings and driver stats. Its weak points are the things
reviewers check first: **no tests, no CI, secrets committed to the repo, a Spanish template
README, synchronous code, and full-table scans.** There are also a few real bugs, two of them
security issues.

---

## 2. Bugs (ranked)

### Security / privacy

| # | Where | Bug | Failure scenario |
|---|---|---|---|
| B1 | `API/Controllers/RegistrationsController.cs:69` + `Business/RegistrationService.cs` | **A driver can get into a private league just by knowing its ID.** Self-registration never checks `IsPublic` or `LeagueViewPolicy`. | A driver calls `POST /leagues/7/registrations` on a private league → 201. They now pass `CanView` and can read its races, results and standings. |
| B2 | `API/Controllers/UsersController.cs:25` + `Models/Dtos/UserDTOOut.cs:14` | **Every user's email address is public.** `GET /users` and `GET /users/{id}` are anonymous and return `Email`. | Anyone can call `GET /users` and collect every account's email. |
| B3 | `API/appsettings.json:10,15`, `docker-compose.yml:33` | The JWT signing key and the SQL `sa` password are committed. | Anyone with the repo can sign an Admin token for any deployment that keeps the default key. |
| B4 | `Models/PasswordHasher.cs` | Passwords are hashed with unsalted SHA-256 (a course requirement). | Identical passwords produce identical hashes, which can be cracked quickly. |
| B5 | `API/Controllers/UsersController.cs:74`, `Models/Dtos/UserDtoIn.cs` | Users choose their own `LicenseClass` at sign-up and on update, and any string up to 10 chars is accepted. | A driver sets themselves to "A" (or "Banana") and gets past any license-based filter. |

### Data integrity / correctness

| # | Where | Bug | Failure scenario |
|---|---|---|---|
| B6 | `Models/User.cs:44-46, 59-61` | **`UndoRaceOutcome` is not the exact inverse of `ApplyRaceOutcome`**, because Safety Rating is clamped to 0–4.99 on both. | A driver at SR 0.20 takes 10x → SR clamps to 0.00. The result is corrected to 0x: undo adds 0.50 → SR 0.50. The driver gained SR from a correction. The same thing happens at the 4.99 cap. |
| B7 | `Business/RegistrationService.cs:31-36` | Capacity and duplicate checks are check-then-insert with no transaction or unique index. | Two concurrent joins to a league with one seat left both succeed (league over capacity), or the same driver is registered twice. |
| B8 | `Business/ResultService.cs:26` | "One result per driver per race" is enforced only in code. There is no unique index on `(RegistrationId, RaceId)`. | Concurrent submissions create duplicate rows → double points and stats. |
| B9 | `API/Controllers/ResultsController.cs:80` | Positions are not unique per race, and points are taken from the client as given. | Two drivers both entered as P1 both get a win. Any points value from 0 to 1000 is accepted. |
| B10 | `Business/LeagueService.cs` `Delete`, `RegistrationService.Delete`, `UserService.Delete` | Multi-step cascades call `SaveChanges` many times with no transaction. | A failure halfway through a league delete leaves some registrations gone and stats only partly reversed. |
| B11 | `API/Controllers/RegistrationsController.cs:69-80` | When an owner registers another driver, `dto.UserId` isn't checked for existence. | `UserId: 9999` → FK violation → `DbUpdateException` → 500 instead of 404. |
| B12 | `API/Controllers/LeaguesController.cs:108` | `MaxDrivers` can be reduced below the current number of registrations. | A league with 20 drivers is set to `MaxDrivers = 5` and silently ends up over capacity. |
| B13 | `Business/AuthService.cs:70` | JWTs are valid for 7 days, and nothing checks whether the user still exists or has the same role. | A deleted user's token can still create a league → FK violation → 500. A demoted admin keeps admin rights for up to 7 days. |
| B14 | Results flow | `Race.IsCompleted` is never set automatically, and results can be entered for races that haven't happened yet. | A race scheduled for next month gets results and stays "not completed". |

### Performance (fine for a demo, a red flag in review)

| # | Where | Issue |
|---|---|---|
| P1 | `Business/AuthService.cs:27,40` | Login and register load **the whole Users table** into memory to compare emails. |
| P2 | `Business/StandingsService.cs:21,27` | Standings load **every result and registration in the database**, not just this league's. |
| P3 | `Business/ResultService.cs:26,74,94,102`, `RegistrationService`, `LeagueViewPolicy` | Same pattern: `GetAll()` followed by LINQ-to-objects. Only the three list endpoints use `IQueryable`. |
| P4 | Everywhere | All database and controller code is synchronous: no `async` and no `CancellationToken`. |
| P5 | List endpoints | No pagination. |

---

## 3. Code quality issues (not bugs, but reviewers will notice)

- **Every action repeats the same `try/catch`** to map exceptions to status codes. A single global
  `IExceptionHandler` that returns RFC 7807 `ProblemDetails` would remove about 40% of the
  controller code and make error responses consistent. Right now errors are a mix of plain
  strings, `ModelState` and empty bodies.
- **Route prefixes are inconsistent:** `/auth`, `/users`, `/Leagues` (`[controller]`),
  `/api/Ping`, plus root-level `/races/{id}` and `/results/{id}`. There is no versioning.
- `GET /leagues/{id}/registrations` is named `GetStandings`. Standings and the member list should
  probably be separate resources.
- `UserDTOOut` leaves out the interesting fields (iRating, SR, license, wins), even though the
  list endpoint lets you filter and sort by them.
- There is no "my leagues" endpoint. A driver can't list the private leagues they own or belong to.
- `DateTime.Now` and `DateTime.UtcNow` are mixed, and there's no timezone handling for
  `ScheduledAt`.
- The `Guest` role exists but nothing ever assigns it.
- `SearchByName` and `SearchByTag` are dead code left over from the CLI.
- Leftover course wording ("DWES2", "Step 20", "San Valero user 28009") in comments, Swagger text
  and the compose file.
- **.NET 8 support ends 2026-11-10 (today).** A portfolio piece on an unsupported runtime looks
  dated. Move to .NET 10 LTS.

---

## 4. Roadmap

### Phase 0: Fix the bugs (small, do first)
1. B1: only allow self-registration into public leagues. Private leagues are join-by-owner only
   (or by invite, see Phase 3).
2. B2: split the DTOs into a public driver profile (name, tag, iRating, SR, license, wins) and a
   private `/me` view that includes the email.
3. B6: store SR changes on each result (an `SrDelta`/`IRatingDelta` column) and reverse by
   subtracting the stored delta, not by recomputing it. This also prepares for Phase 3's rating
   algorithm.
4. B7/B8/B9: add unique indexes on `(UserId, LeagueId)`, `(RegistrationId, RaceId)` and
   `(RaceId, Position)`, and map `DbUpdateException` from a unique-index violation to 409.
5. B10: wrap multi-step service operations in one transaction (a unit of work or
   `BeginTransaction`).
6. B5, B11, B12, B14: validate and reject these cases with clear 4xx responses.

### Phase 1: Engineering foundations (what recruiters check first)
1. **Tests.** xUnit unit tests for the business rules (results back-out, rounds renumbering,
   standings tiebreak, capacity). Integration tests with `WebApplicationFactory` and
   **Testcontainers** (a real SQL Server or Postgres) for auth and authorization rules, including
   a regression test for every bug above. This is the single biggest credibility improvement.
2. **CI:** a GitHub Actions workflow that builds, tests and builds the Docker image on every PR,
   with a badge in the README.
3. **Secrets out of the repo:** user-secrets for development, environment variables or a secret
   store for deployment. Fail at startup if the JWT key is missing or still the default.
4. **Password hashing:** use `Microsoft.AspNetCore.Identity.PasswordHasher<T>` (PBKDF2, salted,
   versioned) and rehash old SHA-256 hashes on login.
5. **Global exception handler + ProblemDetails**, and delete the per-action try/catch blocks.
6. **Async all the way down**, with `CancellationToken` and `IQueryable` repositories (fixes
   P1–P4).
7. **Pagination** (`page`/`pageSize` plus a total count) on every list endpoint.
8. **Consistent `/api/v1/...` routing.**
9. **Upgrade to .NET 10 LTS.**
10. Health checks (`/health`, with the database checked by `AspNetCore.HealthChecks`), structured
    logging (Serilog) and a correlation ID.
11. Rate limiting on `/auth/*` using the built-in `AddRateLimiter`.

### Phase 2: Features that make it interesting (choose 2–3, don't build all of them)
These are what set it apart from a CRUD tutorial. Each one shows a specific backend skill.

| Feature | Skill it shows |
|---|---|
| **Server-side points systems:** each league defines a points table (e.g. F1 25-18-15…, fastest-lap bonus, 0 for DNF). The server computes points from position instead of trusting the client. | Domain modelling, removing trust in client input (fixes B9 properly) |
| **Bulk race-result submission:** `POST /races/{id}/results` with the whole field in one atomic transaction, which validates unique positions and marks the race completed | Transactions, batch validation (fixes B14) |
| **Multi-driver iRating calculation:** an Elo-style rating change based on the field's strength, not the current fixed per-position delta, with stored deltas so corrections replay correctly | A real algorithm, plus testability (a showcase unit-test suite) |
| **Result import from a file (CSV)** | File upload, parsing, validation and reporting per row |
| **Driver stats and history:** `/drivers/{id}/stats` with average finish, incidents per race and an iRating chart series | Aggregation queries and projections |
| **Invites for private leagues** (single-use token, with expiry) | A proper fix for B1, token design |
| **Optimistic concurrency** (`rowversion`) on league and registration edits | Concurrency control (fixes B7 properly) |
| **Refresh tokens + revocation** | Auth beyond the basics (fixes B13) |

**Recommended set:** points systems + bulk results + Elo-style iRating. Together they tell one
story ("results go in, a correct championship comes out"), and that story is the project's
existing strength.

### Phase 3: Publish
1. **Deploy** a live instance with seeded demo data and a public read-only demo account. Leave
   Swagger UI (or Scalar) public on the live site so recruiters can try it in the browser.
2. **README in English:** a pitch, an architecture diagram (Mermaid), a "try it live" link,
   one-command `docker compose up`, a design-decisions section (ADRs:
   service-level cascades vs. DB cascades, stored deltas, why Restrict FKs), and test and CI
   badges. `PORTFOLIO_BRIEF.md` is the source for this.
3. Remove the course wording, then tag `v2.0.0` with a changelog.
4. Optional: a small read-only frontend (standings table plus a driver page). Only worth doing if
   your portfolio needs something visual. For a backend portfolio, the live Swagger and the tests
   carry more weight.

---

## 5. Decisions (answered 2026-11-10)

1. Database: **PostgreSQL**.
2. CLI compatibility: **dropped**.
3. Phase 2: **all features**.
4. Frontend: **none**, API only, shown through Swagger.
5. Class project: **mentioned** in public copy.

The implementation plan is in `SPEC.md`.
