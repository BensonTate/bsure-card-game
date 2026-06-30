using Api.Models;

namespace Api.Services;

// Interface exists for two reasons:
// 1. Lets the API depend on an abstraction (registered in DI as a singleton)
// 2. Lets tests mock or substitute the service if needed, though for a pure
//    class like this, tests just use the real implementation directly.
public interface IScoringService
{
    // Calculates hand score for every player, determines winner(s), and
    // populates SuitProduct only for players involved in a tie-break.
    void ScoreGame(IReadOnlyList<Player> players);
}