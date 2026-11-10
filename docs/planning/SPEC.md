# Spec: iRacing League API v2: from class project to public portfolio API

## Problem Statement

The iRacing League API started as a class project (the DWES module). Its domain logic is solid:
results feed league standings and driver ratings. In its current state, though, it can't be put
in front of recruiters:

- There's no live instance, and the README is a Spanish course template that describes routes
  that don't exist.
- Private leagues aren't really private: any driver who knows a league's id can join it and then
  read everything in it.
- Every account's email address is available to anonymous callers.
- The JWT signing key and database password are committed. Passwords are hashed with unsalted
  SHA-256.
- Results can be inconsistent:
  - two drivers can both be classified P1;
  - points are whatever the client sends;
  - results can be entered for races that haven't happened yet;
  - correcting a result can raise a driver's Safety Rating;
  - multi-step deletes can fail halfway.
- Concurrent joins can push a league past its driver limit, and nothing stops duplicate
  registrations.
- Driver data is only partly protected: drivers can set their own license class, and deleted or
  demoted users keep working tokens for a week.
- It runs on .NET 8, which reaches end of support on 2026-11-10, against SQL Server, which is
  expensive to host.
- Error responses have no consistent shape, routes don't share a prefix, lists aren't paginated,
  and several queries load whole tables into memory.

From the developer's side: the project doesn't yet show the backend depth it could. The
interesting work (keeping a championship correct through every edit) is buried under these
problems, and nothing about it can be tried live.

## Solution

A v2 of the API, deployed publicly with a seeded demo dataset, and shown through Swagger UI.
A visitor can try it in the browser with no setup:

- browse public leagues, standings, race classifications and driver profiles anonymously;
- sign in with a documented demo account to create leagues, schedule races, invite drivers and
  submit results;
- watch standings and ratings update.

Behind that:
- League owners submit a **whole race classification** at once, by JSON or by CSV upload with a
  preview mode.
- The server computes points from the league's own **points system**.
- Driver ratings use an **iRacing-style, Elo-based iRating** plus a Safety Rating and license
  class. They are derived from the full race history and replayed forward whenever history
  changes, so corrections and deletions always leave a correct championship.
- Private leagues can only be joined by invite.
- Authentication uses short-lived access tokens with rotating refresh tokens.
- Concurrent edits are detected instead of silently overwritten.
- The whole API follows one convention set: versioned routes, consistent error documents,
  pagination, and full Swagger documentation.
- It runs on .NET 10 and PostgreSQL, starts locally with one Docker Compose command, and is
  presented by an English README that says it began as a class project.

## User Stories

### Visitor / recruiter (anonymous)
1. As a visitor, I want the API's root URL to open Swagger UI, so that I can explore it with no setup.
2. As a visitor, I want every endpoint documented with a summary, example request bodies and every possible response, so that I understand the API without reading code.
3. As a visitor, I want documented demo credentials, so that I can try the write operations in Swagger right away.
4. As a visitor, I want the live instance to already contain realistic leagues, races, results and drivers, so that every read endpoint returns something interesting.
5. As a visitor, I want to list public leagues filtered by discipline and sorted by name, entry fee, creation date or max drivers, so that I can find a league to look at.
6. As a visitor, I want list endpoints paginated with a total count, so that I can page through results predictably.
7. As a visitor, I want to see a public league's details, race calendar, members and standings, so that I can follow a championship.
8. As a visitor, I want to see a race's full classification ordered by position, including points, fastest lap and rating changes, so that I understand what happened in the race.
9. As a visitor, I want to browse driver profiles filtered by minimum iRating and license class and sorted by iRating, wins or name, so that I can compare drivers.
10. As a visitor, I want a driver's profile to show iRating, Safety Rating, license class, wins and races completed, but never their email, so that profiles are informative without exposing private data.
11. As a visitor, I want a driver's stats (races, wins, podiums, DNFs, average finish, average incidents, fastest laps) and their iRating and Safety Rating history, so that I can see how their career progressed.
12. As a visitor, I want private leagues and their races, results, members and standings to be refused to me, and left out of driver stats, so that private data stays private.
13. As a visitor, I want every error to come back as a consistent problem document with a status, title, details and trace id, so that the API feels professional and predictable.
14. As a visitor, I want to read the README in English with an architecture diagram, the project's origin as a class project, and its main design decisions, so that I understand what was built and why.

### Driver (authenticated)
15. As a new driver, I want to register with a username, email, password and tag, so that I can take part in leagues.
16. As a driver, I want to log in and receive a short-lived access token and a refresh token, so that I stay signed in securely.
17. As a driver, I want to swap my refresh token for a new pair, so that I don't have to log in again every 15 minutes.
18. As a driver, I want to log out and have my refresh token family revoked, so that a stolen token stops working.
19. As a driver, I want login to give the same error for an unknown email and a wrong password, so that nobody can find out which emails are registered.
20. As a driver, I want login and registration to be rate-limited, so that my account is protected against password guessing.
21. As a driver, I want to view my private profile, including my email, so that I can check my account details.
22. As a driver, I want to change my username and tag, so that my profile shows who I am.
23. As a driver, I want my license class, iRating, Safety Rating and win count to be calculated by the system only, so that nobody can fake their standing, me included.
24. As a driver, I want to list every league I own or am registered in, private ones included, so that I can find my championships.
25. As a driver, I want to join a public league with a car number, team name and ballast, so that I can race in it.
26. As a driver, I want to join a private league using an invite token from its owner, so that I can take part in invite-only championships.
27. As a driver, I want to be refused when I try to join a private league without a valid invite, so that private leagues stay closed.
28. As a driver, I want a clear conflict error when the league is full, I'm already registered, or my car number is taken, so that I know why joining failed.
29. As a driver, I want to withdraw from a league, so that I can leave a championship I no longer race in.
30. As a driver, I want my ratings to update after every race based on the strength of the field I beat or lost to, so that beating strong drivers counts for more.
31. As a driver, I want a clean race to raise my Safety Rating and incidents to lower it, so that clean driving is rewarded.
32. As a driver, I want my license class to move from Rookie through D, C, B and A based on my Safety Rating once I have enough races, so that my license reflects how I drive.
33. As a driver, I want my ratings to be recalculated correctly when an earlier race is corrected or removed, so that one mistake doesn't distort my career numbers for good.
34. As a driver, I want to create my own league, so that I can run a championship.

### League owner
35. As a league owner, I want to create a league with a name, discipline, visibility, driver limit, entry fee and points system, so that it runs under my rules.
36. As a league owner, I want a sensible default points system (F1-style, 1-point fastest-lap bonus) when I don't set one, so that creating a league is quick.
37. As a league owner, I want to set a custom points table and fastest-lap bonus, with validation that the table never increases down the order, so that my scoring is valid.
38. As a league owner, I want changing the points system to recalculate the points of every existing result in my league, so that the standings always match the current rules.
39. As a league owner, I want to edit my league's details, so that I can fix mistakes or change settings.
40. As a league owner, I want lowering the driver limit below the current number of members to be refused, so that the league is never over capacity.
41. As a league owner, I want to delete my league along with everything in it, and have every affected driver's ratings recalculated, so that removing a league leaves no inconsistent data.
42. As a league owner, I want to create time-limited, single-use invite tokens, so that I control who joins my private league.
43. As a league owner, I want to list and revoke my league's invites, so that I can cancel invites I've shared by mistake.
44. As a league owner, I want to register another existing driver into my league, with a not-found error if the driver doesn't exist, so that I can manage the grid myself.
45. As a league owner, I want to remove any member from my league, so that I can manage the grid.
46. As a league owner, I want to schedule races with track, car, date, laps and ambient temperature, so that my league has a calendar.
47. As a league owner, I want round numbers assigned automatically from the schedule order and kept continuous when races are added, moved or removed, so that I never have to number rounds myself.
48. As a league owner, I want to edit or delete a scheduled race, so that I can keep the calendar accurate.
49. As a league owner, I want rescheduling a race that already has results to be refused, so that the order of rated races can't be changed after the fact.
50. As a league owner, I want to submit a race's full classification in one request, so that the whole field is scored together and atomically.
51. As a league owner, I want a classification rejected with every problem listed (duplicate or missing positions, duplicate drivers, drivers from another league, DNFs placed above finishers, a race in the future, values out of range), so that I can fix everything in one go.
52. As a league owner, I want the server to calculate points and award the fastest-lap bonus, so that drivers can trust the scoring.
53. As a league owner, I want submitting the classification again to replace the previous one completely, so that corrections are simple and the result is the same however many times I submit.
54. As a league owner, I want submitting a classification to mark the race as completed, and clearing it to mark the race as not completed, so that race status is always accurate.
55. As a league owner, I want to upload a CSV of results (position, car number, fastest lap, incidents, DNF, notes), so that I can import results without typing JSON.
56. As a league owner, I want a CSV preview mode that shows the calculated classification without saving it, so that I can check an import before committing it.
57. As a league owner, I want CSV errors reported for every bad row and field, so that I can fix the file in one pass.
58. As a league owner, I want lap times in a CSV accepted either as seconds or as minutes:seconds, so that I can paste times in whatever format I have.
59. As a league owner, I want standings sorted by points, then wins, then fewest incidents, so that ties are broken fairly.
60. As a league owner, I want my edit refused with a precondition-failed error if someone else changed the league or race since I loaded it, so that I don't overwrite their changes without knowing.

### Admin
61. As an admin, I want to manage any league, race, membership or classification as if I owned it, so that I can moderate the platform.
62. As an admin, I want to delete a driver account, with their memberships and results removed and ratings recalculated, so that I can remove abusive or test accounts cleanly.
63. As an admin, I want deleting a driver who still owns leagues to be refused, so that leagues never lose their owner.
64. As an admin, I want a refresh to fail for deleted users and new access tokens to always carry the user's current role, so that removed or demoted users lose access within minutes.

### Developer / operator
65. As the developer, I want `docker compose up` on a clean machine to start PostgreSQL and the API with schema and demo data ready, so that anyone can run the project locally.
66. As the developer, I want no secrets in the repository and the app to refuse to start without a strong signing key, so that a published repo can't be used to forge tokens.
67. As the developer, I want an example environment file committed and the real one ignored by git, so that configuration is obvious but private.
68. As the developer, I want liveness and readiness health endpoints, the readiness one checking the database, so that the host can monitor the deployment.
69. As the developer, I want CI to build the solution and the Docker image on every pull request, so that broken builds never reach main.
70. As the operator, I want a demo mode that reseeds on startup and resets the data daily, so that the live instance stays clean however visitors use it.
71. As the operator, I want the app to read the real client IP behind the host's proxy, so that rate limiting works in production.
72. As the developer, I want structured JSON logs and a trace id on every error, so that I can debug problems in the live instance.
73. As the developer, I want architecture decision records for the key design choices, so that reviewers can see the reasoning, not just the code.
74. As the developer, I want the course leftovers (course wording, dead CLI code, unused role, outdated Postman collection, ping endpoint) removed, so that the codebase reads as a finished product.
75. As the developer, I want a tagged v2.0.0 release with a changelog, so that the portfolio links to a clear, stable version.

## Implementation Decisions

### Platform
- The target is **.NET 10 LTS**, with EF Core 10 and the Npgsql provider. Check package versions when implementing.
- Data moves to **PostgreSQL**. The existing SQL Server migrations are discarded and replaced with one fresh initial migration. There is no production data to keep.
- Compatibility with the original CLI is **dropped**. The business layer becomes async and transactional.
- All timestamps are UTC, stored as timestamp-with-time-zone. Inputs without an offset are treated as UTC. Local-time timestamps are removed (Npgsql rejects them).
- Docker Compose runs PostgreSQL 17 with a healthcheck and reads its secrets from a git-ignored environment file.
- The deployment doesn't depend on any particular host: a container image, environment variables and a Postgres connection string. The host is chosen when publishing.

### Architecture
- The five-module layered structure stays: Models, Data, Data.EF, Business, API. This is the README's "class project, then extended" story.
- **Business stays free of EF.** Repository interfaces expose specific async query methods (e.g. find by email, standings for a league, results for a race) instead of query objects, so database access stays in Data.EF and nothing loads whole tables (fixes the in-memory scans).
- A **unit-of-work** abstraction in Data offers "save changes" and "run this work in a transaction". The EF implementation wraps the work in the provider's execution strategy. Every operation that writes more than one row runs in one transaction.
- Calculation logic lives in self-contained Business modules with no database access: **points calculator**, **rating calculator**, **license rules**, **CSV result parser**. Each takes data in and returns data out.
- Everything is async and takes a cancellation token, from controllers to repositories.
- Controllers contain **no try/catch**. Business throws typed domain exceptions, and one global exception handler turns them into problem documents.
- Dead code is removed: the CLI-era search helpers, unused list methods, the Guest role, the CLI string formatting, the ping endpoint and the Postman collection.

### API contract
- All resource routes live under `/api/v1`: lower-case, plural, nested where a resource belongs to another. Health endpoints aren't versioned.
- Error mapping:

  | Error | Status |
  |---|---|
  | Not found | 404 |
  | Forbidden | 403 |
  | Conflict (duplicate registration, league full, user owns leagues, unique-constraint violation) | 409 |
  | Domain validation | 400, with a field → errors map |
  | Stale concurrency token | 412 |
  | Rate limited | 429 |
  | Anything else | 500, logged, nothing leaked |

  Every problem document includes a trace id.
- Pagination: `page` (default 1) and `pageSize` (default 20, max 100). List responses return items, page, page size and total count.
- Filtering and sorting run in the database. The parameters stay as in v1.
- Swagger is the showcase:
  - doc comments and every response type on every action;
  - example bodies;
  - grouped tags;
  - a bearer-token button;
  - served in every environment;
  - the root redirects to it.

- Endpoints:

| Endpoint | Access | Behaviour |
|---|---|---|
| Register / login | anonymous, rate-limited | Returns access token, refresh token, expiry |
| Refresh | anonymous, rate-limited | Rotates the refresh token |
| Logout | anonymous | Revokes the refresh token family |
| Me: get / update / my leagues | authenticated | Private profile with email. Update covers username and tag only. Leagues owned or joined |
| Drivers: list / get / stats | anonymous | Public profile, never email |
| Drivers: delete | admin | Conflict if the driver owns leagues |
| Leagues: list | anonymous | Public only, enforced by the server |
| Leagues: create | driver or admin | Owner taken from the token. Optional points system |
| Leagues: get | league view policy | Returns an ETag |
| Leagues: update / delete | owner or admin | Update accepts an optional If-Match |
| League members: list | league view policy | |
| League members: join / add | authenticated | See Membership |
| League members: remove | self, owner or admin | |
| League standings | league view policy | |
| League invites: create / list / revoke | owner or admin | The plain token is returned only once |
| League races: list | league view policy | |
| League races: create | owner or admin | |
| Race: get | league view policy | Returns an ETag |
| Race: update / delete | owner or admin | Update accepts an optional If-Match. The completed flag isn't client-settable |
| Race results: get | league view policy | Ordered by position |
| Race results: replace classification | owner or admin | See Classification |
| Race results: CSV import | owner or admin | Optional preview mode |
| Race results: clear | owner or admin | |
| Health: live / ready | anonymous | Ready checks the database |

- The v1 endpoints for creating, editing or deleting a single result are removed. Elo changes depend on the whole field, so the classification is the unit of change.
- **League view policy:** a public league is visible to everyone. A private league is visible to its owner, admins and registered members only.

### Schema
- **User:**
  - stores a password hash produced by the framework's salted, versioned password hasher;
  - roles are Admin and Driver;
  - iRating, Safety Rating, license class, total wins and races completed are **derived caches**, written only by the rating replay.
- **League:** gains a points table (an integer array, a native Postgres array) and a fastest-lap bonus. It gets a row-version concurrency token mapped to Postgres's `xmin`.
- **Race:** the completed flag is set by the system only. It gets the same row-version concurrency token.
- **Registration:**
  - loses its stored points; standings are computed;
  - unique per (league, driver) and per (league, car number).
- **Result:**
  - points are computed by the server;
  - gains a fastest-lap flag, iRating before, iRating change, Safety Rating before and Safety Rating change;
  - loses its separate finish timestamp (the race date is the time);
  - unique per (race, registration) and per (race, position).
- **League invite (new):** league, token hash, creator, created, expires, used-at, used-by, revoked-at.
- **Refresh token (new):** user, token hash, family id, created, expires, revoked-at, replaced-by.
- **Deletes:**
  - cascade in the database: league → races, members, invites; race → results; registration → results; user → registrations, refresh tokens;
  - restricted: user → owned leagues.

  Postgres allows the multiple cascade paths SQL Server refused, and derived ratings are recalculated by replay, so service-level cascade code is no longer needed.
- Invite and refresh tokens are 32 random bytes from a secure generator, base64url-encoded. Only their SHA-256 hash is stored, which is acceptable for high-entropy random values (unlike passwords).

### Classification
- A classification is a list of entries: registration, position, fastest lap in seconds (0 = no lap time), incidents, DNF and notes.
- It's rejected with every problem listed when:
  - the race is scheduled in the future;
  - the list is empty or has more entries than the league has members;
  - positions aren't exactly 1..N;
  - a registration repeats or belongs to another league;
  - a DNF is placed above a finisher;
  - a value is out of range (v1 ranges are kept).
- If it's accepted, one transaction:
  1. deletes the race's previous results;
  2. stores the new ones with server-calculated points;
  3. marks the race completed;
  4. replays ratings from this race.

  The operation gives the same result however many times it's sent.
- Clearing a classification deletes the results, marks the race not completed, and replays ratings.

### Points
- A finisher scores the table value for their position, or 0 if they finish beyond the end of the table. A DNF scores 0.
- The fastest-lap bonus goes to the finisher with the lowest lap time above zero. Ties go to the better position.
- Table rules: 1–50 entries, each 0–1000, never increasing down the order. Bonus 0–50.
- Default table: 25, 18, 15, 12, 10, 8, 6, 4, 2, 1, with a bonus of 1.
- Example: P1 with the fastest lap scores 26, P11 scores 0, and a DNF classified P3 scores 0 and can't take the bonus.
- Changing a league's points system recalculates the points on all its results in one transaction. Ratings don't change.

### Ratings
These are iRacing-*style* ratings. Public copy must not claim they are iRacing's real algorithm.
- **iRating:** pairwise Elo over the whole field.
  - Each driver's expected score is the sum, over every other driver, of 1 / (1 + 10^((their rating − my rating)/400)).
  - The actual score is the number of drivers finishing behind.
  - The change is K × (actual − expected) / (N − 1), rounded, with K = 200. A field of one gives no change. iRating never goes below 0.
  - Example: three drivers at 1350 finishing 1-2-3 get +100, 0 and −100.
- **Safety Rating:** change per race = 0.10 − 0.05 × incidents, keeping the result between 0.00 and 4.99. The *actual* change applied is stored. That is why a correction can no longer push a driver's rating up.
- **Wins:** P1 and not a DNF.
- **License:** fewer than 5 races → Rookie. Otherwise Safety Rating below 2.0 → D, below 3.0 → C, below 4.0 → B, else A.
- Base values: iRating 1350, Safety Rating 2.50.
- K, the SR formula and the license thresholds are defaults chosen during planning, not values the user picked. They are kept as named constants so they're easy to tune.

### Rating replay
- Ratings are a function of race history. Any change to history recalculates everything after the change: submitting, clearing or importing a classification, and any delete that removes results.
- Races are ordered by (scheduled date, race id) across **all leagues**, because iRating is global.
- Steps:
  1. Take a database-wide advisory lock so two replays can't interleave.
  2. Start each affected driver from the values recorded on their last result before the starting race, or from the base values if there isn't one.
  3. Process every completed race from the starting race onward, in order, storing each result's before values and changes.
  4. Update the cached values on every touched driver. Drivers left with no results go back to the base values.
- For deletes, the starting race is the earliest completed race that lost results.

### Membership
- Self-join is allowed for public leagues. A private league requires a valid invite: it belongs to this league, hasn't expired, hasn't been used and hasn't been revoked. The invite is used up in the same transaction.
- Adding another driver is limited to the owner or an admin, and the driver must exist.
- Capacity is checked after locking the league row for update inside the transaction, so concurrent joins can't go over the limit. Unique constraints are the last line of defence against a duplicate driver or car number.
- Lowering a league's driver limit below its current number of members is refused.

### Standings
- Computed by one grouped database query per league. Per registration: points, races, wins, podiums, incidents and best finish, plus the driver's public name and tag.
- Ordered by points (desc), wins (desc), incidents (asc), then registration id.
- Each row carries its 1-based position.

### Driver stats
- Totals: races, wins, podiums, DNFs, average finish, average incidents, fastest laps.
- An iRating history and a Safety Rating history, one point per race, built from the stored before/change values.
- Only results from public leagues and from private leagues the caller can view are included.

### CSV import
- Upload limits: one file, at most 1 MB and 100 rows.
- A header row is required: position, car number, fastest lap, incidents, DNF, notes. Parsed with the CsvHelper library, so quoted notes work.
- Fastest lap is accepted as seconds, minutes:seconds or empty. DNF is accepted as true/false/1/0 or empty. The car number is matched to a registration in the race's league.
- Every row and field error is returned together, each with its row number.
- Preview mode returns the calculated classification and saves nothing. Otherwise the import goes through exactly the same path as a classification submission.

### Auth
- Access tokens last 15 minutes and keep v1's claims: user id, name, role, email.
- Refresh tokens last 14 days and rotate on every use. Reusing a revoked token revokes its whole family. A refresh fails for deleted users and always issues the user's current role.
- `/auth/*` is rate-limited per client IP: a fixed window of 10 requests per minute. The forwarded-headers setup makes the client IP correct behind a proxy.
- The signing key and connection string come only from configuration: user-secrets in development, environment variables elsewhere. Startup fails if the key is missing or shorter than 32 bytes. Nothing secret is committed.
- No legacy password migration is needed, because the database is new.

### Optimistic concurrency
- League and race reads return an ETag built from the row version.
- An update with a mismatched If-Match header gets 412. A conflict detected by the database at save time also gets 412.
- If-Match is optional, so the API stays easy to use from Swagger.

### Rounds
- v1 behaviour stays: rounds are 1..N in schedule order (date, then id). They are renumbered inside the same transaction after a race is created, moved or deleted.

### Demo & publishing
- The demo seeder goes through the real business services, so seeded ratings come from seeded results. It creates:
  - about 12 drivers, with documented demo logins;
  - two public leagues and one private league;
  - a mix of completed and upcoming races.
- In demo mode the seeder runs on startup, and a background service resets the data daily.
- The README is in English and covers:
  - the pitch;
  - its origin as a class project (DWES module), then extended;
  - a Mermaid architecture diagram;
  - the live Swagger link and demo logins;
  - the one-command local start.
- Architecture decision records cover:
  1. Postgres with ratings derived by replay;
  2. storing applied changes instead of reversing formulas;
  3. the full classification as the unit of change;
  4. a pessimistic lock for capacity and optimistic concurrency for edits;
  5. refresh token rotation.
- Course leftovers are removed from code, comments and Swagger text. The portfolio brief is updated to match v2, and the internal planning documents are moved out of the repo root. Release v2.0.0 with a changelog.
- CI builds the solution and the Docker image on every pull request.

## Testing Decisions

- **No automated tests are part of this spec, by the developer's decision.** No test projects, test containers or test steps in CI.
- A good test would check external behaviour through the API, not internal details. That's the approach to take if tests are added later; see Further Notes.
- Verification is manual: through Swagger UI against a local `docker compose up` and against the live instance, using the seeded demo accounts. Each user story above is an acceptance criterion to check by hand.
- Scenarios worth checking by hand because they are easy to get wrong:
  - joining a private league without an invite;
  - two concurrent joins for the last seat;
  - an invalid classification that should report every error;
  - correcting an earlier race and confirming later races' ratings change;
  - deleting a league that has results;
  - reusing a refresh token;
  - a stale If-Match;
  - a CSV preview vs. a real import.
- There are no existing tests in the codebase to copy from.

## Out of Scope

- Automated tests of any kind, and test steps in CI.
- Any frontend. Swagger UI is the only interface.
- Compatibility with the original CLI application.
- Migrating existing v1 data or password hashes.
- Changing a user's role, changing email, password reset and email verification.
- Seasons or championships spanning several leagues, team standings, and penalties applied after a race.
- Pulling data from iRacing's real services. CSV import accepts the documented columns, not iRacing's export format.
- Choosing and paying for hosting beyond what's needed to publish one demo instance.
- Snake_case database naming and other cosmetic schema conventions.

## Further Notes

- **Delivery order.** Each step is one PR and leaves main deployable:
  1. platform (runtime, Postgres, secrets, compose, health, CI, cleanup);
  2. API conventions (routes, errors, async repositories, pagination, profile split, unit of work, Swagger docs);
  3. auth (hasher, refresh tokens, rate limiting);
  4. membership integrity (unique constraints, capacity lock, invites);
  5. results engine (points systems, classification, standings, rating replay, cascading deletes);
  6. CSV import;
  7. driver stats and "my leagues";
  8. optimistic concurrency;
  9. publishing (demo seeding and reset, deployment, README, ADRs, release).
- **Testing caveat.** The original review named "no automated tests" as the biggest credibility gap for a backend portfolio. Leaving tests out is a deliberate choice. If it's revisited, the recommended approach is a single seam: end-to-end tests through the HTTP API against a real Postgres database.
- The project is presented as backend and REST API work, shown through Swagger.
- The bug list and the reasoning behind these decisions come from the code review done on 2026-11-10 at commit `7cadbe5`.
