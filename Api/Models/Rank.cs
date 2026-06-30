namespace Api.Models;

// Sequential values (Two=2 ... Ace=14) so every enum member is unique.
// Uniqueness matters because we persist Rank as an int in SQLite and cast
// it back — if two members shared a value, the cast would be ambiguous and
// one would become unreachable through (Rank)intValue.
//
// Card SCORING (Ace=11, not 14) is a game rule, not part of the card's
// identity, so it lives in RankExtensions.GetCardValue() — not in the enum.
// That separation means a rule change (e.g. Ace=1) is a one-method edit.
public enum Rank
{
    Two = 2,
    Three = 3,
    Four = 4,
    Five = 5,
    Six = 6,
    Seven = 7,
    Eight = 8,
    Nine = 9,
    Ten = 10,
    Jack = 11,
    Queen = 12,
    King = 13,
    Ace = 14
}