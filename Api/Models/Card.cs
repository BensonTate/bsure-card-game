namespace Api.Models;

// EF entity representing a single dealt card.
// A card has no behaviour beyond its identity — value comes from Rank.GetCardValue().
//
// DeckId (1 or 2) is essential: the brief uses two combined 52-card decks,
// so the same (Rank, Suit) pair CAN appear twice across a game. Without
// DeckId, those two cards would be indistinguishable rows in the database.
public class Card
{
    public int Id { get; set; }
    public Rank Rank { get; set; }
    public Suit Suit { get; set; }
    public int DeckId { get; set; }          // Either 1 or 2.

    // Foreign key to the owning player.
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;   // Navigation property, set by EF.
}