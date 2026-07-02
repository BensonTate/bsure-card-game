# Card Game — Take-home Assessment

Full-stack multiplayer card game. Six players are dealt five cards each from two combined 52-card decks, hands are scored, and winners are determined — with a suit-product tie-break when hand scores are tied.

## Stack

- **Backend:** ASP.NET Core Minimal API on .NET 8, Entity Framework Core, SQLite
- **Frontend:** Plain HTML, vanilla JavaScript, CSS (no framework, no build step)
- **Tests:** xUnit + FluentAssertions

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) (`dotnet --version` should report `8.x.x`)
- Python 3 (only needed to serve the static frontend locally — `python3 --version`)
- A modern browser

No database server to install — SQLite is a single file created on first run.

## Getting started

Clone and change into the repo:

```bash
git clone https://github.com/BensonTate/bsure-card-game.git
cd bsure-card-game
```

### 1. Run the tests

```bash
dotnet test
```

Expected: `Passed! 28, Failed: 0` covering scoring, tie-breaks, deck integrity, and re-deal state reset.

### 2. Run the API

In one terminal:

```bash
dotnet run --project Api
```

Kestrel starts on `http://localhost:5155`. First run creates `Api/cardgame.db` (SQLite file, gitignored) via `EnsureCreated`. Confirm with:

```bash
curl http://localhost:5155
# → Card Game API
```

Leave this terminal running.

### 3. Run the frontend

In a second terminal:

```bash
cd frontend
python3 -m http.server 8000
```

Open [http://localhost:8000](http://localhost:8000) in a browser.

Click **Deal** to start a game. **Re-deal** replays the current game with a fresh shuffle. **Past Games** shows a paginated history.

> The API base URL is hard-coded near the top of `frontend/app.js` as `const API_BASE = "http://localhost:5155"`. Change it there if Kestrel picks a different port.

## API endpoints

| Method | Path                       | Description                                                    |
|--------|----------------------------|----------------------------------------------------------------|
| POST   | `/games`                   | Deal a new game, score it, persist, return full game (201)     |
| GET    | `/games/{id}`              | Fetch a game with all players and cards (200 / 404)            |
| GET    | `/games?page=1&pageSize=10`| Paginated list of game summaries, newest first                 |
| POST   | `/games/{id}/redeal`       | Re-deal an existing game, re-score, persist, return (200 / 404)|

Errors are returned as RFC 7807 `ProblemDetails` JSON with a real `detail` message the frontend surfaces as a toast.

## Project structure

## Design decisions and trade-offs

- **Schema deviation from the brief.** Score fields (`HandScore`, `SuitProduct`, `IsWinner`) live on `Player` rather than in a separate `Scores` table. Scores have no independent existence — strictly 1:1 with a player — so a separate table would mean an extra join on every read for no benefit.
- **Two decks are modelled with a `DeckId` column on `Card`** (values `1` or `2`), not a separate `Decks` table. There's no data associated with a deck beyond its identity, so a table would be pure ceremony.
- **`Rank` enum uses sequential values** (`Two=2`…`Ace=14`) so members are unique and safe to cast to/from int. Scoring (Ace = 11, not 14) is a game rule and lives in a separate `RankExtensions.GetCardValue()` method — the card's identity and its value in this specific game are separate concerns.
- **`SuitProduct` is nullable.** It's only calculated for players involved in a tie-break. `null` means "not applicable" — the frontend uses that signal to hide the suit-product display for non-tied players. A default of zero would be ambiguous with a real calculated value.
- **DTOs are records, kept separate from EF entities.** Serializing entities directly would cause circular-reference JSON errors (via navigation properties) and couples the API contract to the database schema.
- **Randomness is behind `IRandomProvider`.** Production uses `Random.Shared`; tests inject a seeded implementation so deals are deterministic and assertable. This is what makes cases like "same-rank-different-deck" testable rather than relying on statistical hope.
- **`EnsureCreated` instead of EF migrations.** Migrations exist to evolve a schema over time without losing data. This DB is throwaway — every clone gets a fresh one. Migrations would be ceremony with no benefit for the assessment.
- **CORS is wide open** (`AllowAnyOrigin`) for local development convenience. A production deployment would whitelist the known frontend origin.
- **Global exception middleware** catches any unhandled exception and returns `ProblemDetails` with the real error message (dev) or a generic one (prod). Errors are never silently swallowed.
- **No Docker, no CI, no auth, no ORM helpers like AutoMapper.** The brief explicitly weights "no over-engineering" — the stack is deliberately proportionate to the problem.

## Known limitations

- No optimistic concurrency on redeal — concurrent redeals of the same game are last-write-wins. With more time I'd add a `RowVersion` column.
- No integration tests via `WebApplicationFactory`. The scoring and dealing services carry the tested logic; endpoints are thin orchestration. Integration tests would be next-level polish, deliberately skipped to stay within scope.
- Frontend is served via `python3 -m http.server` for simplicity. Any static-file server (or opening `index.html` directly) would also work given CORS is open.