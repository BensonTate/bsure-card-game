namespace Api.Models;

public static class RankExtensions
{
    // Maps a Rank to its scoring value for THIS game.
    // Per the brief: number cards = face value, J=11, Q=12, K=13, A=11 (not 1).
    // Kept out of the enum so the card's identity (what it is) stays separate
    // from the game's rules (what it's worth here).
    public static int GetCardValue(this Rank rank) => rank switch
    {
        Rank.Ace => 11,
        Rank.King => 13,
        Rank.Queen => 12,
        Rank.Jack => 11,
        _ => (int)rank   // Number cards 2-10 use their enum value directly.
    };
}