using Api.Models;

namespace Api.Services;

public class ScoringService : IScoringService
{
    public void ScoreGame(IReadOnlyList<Player> players)
    {
    
        ArgumentNullException.ThrowIfNull(players);
        if (players.Count == 0)
            throw new ArgumentException("Player list cannot be empty.", nameof(players));

        // Step 1: Calculate each player's hand score.
        // Reset SuitProduct and IsWinner so re-scoring (e.g. after a redeal)
        foreach (var player in players)
        {
            ValidateHand(player);
            player.HandScore = player.Cards.Sum(c => c.Rank.GetCardValue());
            player.SuitProduct = null;
            player.IsWinner = false;
        }

        // Step 2: Find the highest hand score and the players who share it.
        // Max() is safe here because we've already verified the list is non-empty.
        var topScore = players.Max(p => p.HandScore);
        var topPlayers = players.Where(p => p.HandScore == topScore).ToList();

        // Step 3: Single winner — no tie, no suit product needed.
        // SuitProduct stays null for everyone, which is the frontend's
        // signal not to render the suit-score column.
        if (topPlayers.Count == 1)
        {
            topPlayers[0].IsWinner = true;
            return;
        }

        // Step 4: Tie-break. Calculate suit product ONLY for tied players,
        // per the brief: "recalculate scores for tied players only".
        // Non-tied players keep SuitProduct = null.
        foreach (var player in topPlayers)
        {
            player.SuitProduct = player.Cards
                .Aggregate(1L, (product, card) => product * (int)card.Suit);
        }

        // Step 5: Find the highest suit product among tied players.
        // If multiple players share the highest suit product, they all win
        var topSuitProduct = topPlayers.Max(p => p.SuitProduct!.Value);
        foreach (var player in topPlayers.Where(p => p.SuitProduct == topSuitProduct))
        {
            player.IsWinner = true;
        }
    }

    // Each player must have exactly 5 cards. Anything else means the dealer
    // produced an invalid state, which we surface loudly rather than scoring
    // a malformed hand and returning a misleading result.
    private static void ValidateHand(Player player)
    {
        if (player.Cards.Count != GameConstants.CardsPerPlayer)
        {
            throw new ArgumentException(
                $"Player {player.SeatNumber} has {player.Cards.Count} cards; expected {GameConstants.CardsPerPlayer}.",
                nameof(player));
        }
    }
}