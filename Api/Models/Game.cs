namespace Api.Models;

// EF entity representing one round of the card game.
// A Game owns its Players (cascade delete in the DbContext config) and is
// the root aggregate persisted by POST /games.
public class Game
{
    public int Id { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Always 6 players per the brief. Enforced by DealingService, not by
    // the type system — keeping it as a list rather than a fixed-size
    // structure makes EF mapping and JSON serialization straightforward.
    public List<Player> Players { get; set; } = new();
}