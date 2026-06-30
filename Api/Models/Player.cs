namespace Api.Models;

// EF entity for one of the 6 players in a game.
//
// Score fields live directly on Player rather than in a separate Scores table:
// scores have a strict 1:1 relationship with players and never exist on
// their own. The brief's suggested schema is explicitly optional ("design
// the schema that makes sense") — this deviation is documented in the README.
public class Player
{
    public int Id { get; set; }
    public int SeatNumber { get; set; }              // 1-6, used for display order.
    public string Name { get; set; } = string.Empty; // e.g. "Player 1"; kept simple.

    // Foreign key to the game this player belongs to.
    public int GameId { get; set; }
    public Game Game { get; set; } = null!;

    // The 5 cards dealt to this player.
    public List<Card> Cards { get; set; } = new();

    // Sum of card values per the scoring rules.
    public int HandScore { get; set; }

    // Product of suit values — but ONLY populated for players involved in a
    // tie-break. Null means "not applicable" (this player wasn't tied), which
    // is the signal the frontend uses to decide whether to show the suit-score
    // column. A default of 0 would be ambiguous with a real computed value.
    public long? SuitProduct { get; set; }

    // True for sole winner OR any of multiple joint winners (rules allow
    // joint winners when the tie-break itself ties).
    public bool IsWinner { get; set; }
}