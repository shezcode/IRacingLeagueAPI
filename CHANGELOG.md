# Changelog

Behavioral changes made to the CLI app. The API version (same layered structure:
Models / Data / Business) should replicate the same business-layer rules so
both apps behave identically.

## Standings: show races completed per driver

- `IStandingsService.GetStandings(leagueId)` now returns `IEnumerable<StandingEntry>`
  instead of `IEnumerable<Registration>`.
- `StandingEntry` is a new record: `(Registration Registration, int RacesCompleted)`.
- `RacesCompleted` = total number of `Result` records for that registration
  (across all races in the league), including DNFs.
- Standings are still ordered by `Points` descending, then by total incident
  points ascending (tiebreak), same as before.

## Login now requires password verification

- Previously, selecting a driver from the login list immediately granted access
  — no password check.
- Passwords are now stored as SHA256 hashes (`PasswordHasher.Hash`, hex-encoded,
  lowercase).
- `UserService.Create` hashes the supplied password before persisting the user.
- New `IUserService.VerifyPassword(User user, string password)` — hashes the
  input and compares to the stored hash.
- Login flow: select driver -> prompt for password -> `VerifyPassword`. If it
  fails, show an error and do **not** authenticate (no session/active user set).
- Account creation now requires a non-empty password (previously optional/blank).

## League owners can register other drivers ("Add driver")

- New capability, restricted to the league owner: register any driver who is
  not already a member of the league.
- Reuses the existing registration rules (`RegistrationService.Register`):
  - Fails if the driver is already registered in the league
    (`DuplicateRegistrationException`).
  - Fails if the league is at `MaxDrivers` capacity (`LeagueFullException`).
- Input: target user id, car number, team name, ballast (kg) — same fields as
  a normal self-registration.

## Race rounds are auto-numbered from schedule order

- Removed the manual "Round" input when creating a race —
  `Race` no longer takes a `round` in its constructor, and
  `IRaceService.Create(...)` no longer takes a `round` parameter.
- After every race **create** or **delete** in a league, all of that league's
  races are renumbered: ordered by `ScheduledAt` ascending (ties broken by
  `RaceId` ascending), and assigned `Round = 1, 2, 3, ...` in that order.
- This means `Round` is always derived, never user-entered, and always
  contiguous/unique per league.

## Result entry: validation + no duplicate results per driver/race

- Before recording a result, both ids are now validated:
  - The race id must exist **and** belong to the current league.
  - The registration id must exist **and** belong to the current league.
  - If either check fails, show an error and abort — nothing is persisted.
- `ResultService.ApplyResult(registration, race, result)`:
  - Looks for an existing `Result` with the same `RegistrationId` + `RaceId`.
  - **If one exists**: update it in place (Position, FastestLapSeconds, Points,
    IncidentPoints, Dnf, Notes, FinishedAt) instead of inserting a new row.
    Before applying the new values, back out the previous contribution:
    - `registration.Points += (newPoints - oldPoints)`
    - `user.UndoRaceOutcome(oldPosition, oldIncidentPoints)` — exact inverse of
      `ApplyRaceOutcome` (reverses the iRating delta, decrements `TotalWins` if
      the old position was 1, and reverses the safety rating adjustment,
      clamped to [0, 4.99]).
    - Then `user.ApplyRaceOutcome(newPosition, newIncidentPoints)` is applied
      as normal.
  - **If none exists**: behaves as before — add the result, `registration.AddPoints(points)`,
    `user.ApplyRaceOutcome(position, incidentPoints)`.
  - New `User.UndoRaceOutcome(position, incidentPoints)` model method (inverse
    of `ApplyRaceOutcome`).
