using Api.Models;

namespace Api.Services;

// Responsible for producing a fresh Game with 6 players and 5 cards each,
// dealt from two combined 52-card decks (104 cards total) without replacement.
public interface IDealingService
{
    Game DealNewGame();
    void RedealGame(Game existingGame);
}