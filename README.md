# Card Game

Multiplayer card game: 6 players, 5 cards each, dealt from two combined
standard decks. Highest hand score wins, with a suit-product tie-break.

All scoring and dealing logic lives in the API. The frontend only renders
what the API returns.

## Stack

- API: ASP.NET Core Minimal API (.NET 8)
- Database: SQLite via EF Core
- Frontend: plain HTML / CSS / JavaScript (no build step)
- Tests: xUnit

## Project layout

```
Api/            Minimal API project
Api.Tests/      xUnit tests
frontend/       static HTML/CSS/JS, served separately
```

## Prerequisites

(filled in once the API and frontend are running end to end)

## Running locally

(filled in once the API and frontend are running end to end)

## Game rules

- 6 players, 5 cards each, two 52-card decks combined (104 cards), no
  replacement within the combined shoe.
- Hand score: number cards at face value, J=11, Q=12, K=13, A=11.
- Highest hand score wins.
- Tie-break: tied players' suit values are multiplied together
  (♦=1, ♥=2, ♠=3, ♣=4). Highest suit product wins among the tied players.
- If the suit product also ties, the tied players are joint winners.

## API

(filled in once endpoints are implemented)

## Troubleshooting

(filled in at the end)
