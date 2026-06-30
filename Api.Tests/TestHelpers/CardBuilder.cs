using Api.Models;

namespace Api.Tests.TestHelpers;

// Small helper to build cards and players concisely in tests.
// Without this, every test would be cluttered with object-initializer
// boilerplate that obscures the actual scenario being tested.
public static class CardBuilder
{
    public static Card Card(Rank rank, Suit suit, int deckId = 1) =>
        new() { Rank = rank, Suit = suit, DeckId = deckId };

    public static Player Player(int seat, params Card[] cards) =>
        new()
        {
            SeatNumber = seat,
            Name = $"Player {seat}",
            Cards = cards.ToList()
        };
}