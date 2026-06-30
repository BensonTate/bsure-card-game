using Api.Models;

namespace Api.Services;

public class DealingService : IDealingService
{
    private readonly IRandomProvider _random;

    public DealingService(IRandomProvider random)
    {
        _random = random;
    }

    public Game DealNewGame()
    {
        var game = new Game();

        // Create 6 players up front with seat numbers 1..6.
        // The deal then assigns cards to these players, not the other way around.
        for (var seat = 1; seat <= GameConstants.PlayerCount; seat++)
        {
            game.Players.Add(new Player
            {
                SeatNumber = seat,
                Name = $"Player {seat}"
            });
        }

        DealCardsTo(game.Players);
        return game;
    }

    public void RedealGame(Game existingGame)
    {
        ArgumentNullException.ThrowIfNull(existingGame);

        foreach (var player in existingGame.Players)
        {
            player.Cards.Clear();
            // Reset scoring state too ScoringService will recompute,
            // but clearing here avoids any window where stale values are visible.
            player.HandScore = 0;
            player.SuitProduct = null;
            player.IsWinner = false;
        }

        DealCardsTo(existingGame.Players);
    }

    private void DealCardsTo(IReadOnlyList<Player> players)
    {
        var deck = BuildTwoDecks();

        ShuffleInPlace(deck);

        var dealIndex = 0;
        foreach (var player in players)
        {
            for (var c = 0; c < GameConstants.CardsPerPlayer; c++)
            {
                deck[dealIndex].PlayerId = 0;  // Set by EF on insert.
                player.Cards.Add(deck[dealIndex]);
                dealIndex++;
            }
        }
    }

    private static List<Card> BuildTwoDecks()
    {
        var deck = new List<Card>(capacity: 104);

        var allRanks = Enum.GetValues<Rank>();
        var allSuits = Enum.GetValues<Suit>();

        for (var deckId = 1; deckId <= GameConstants.DeckCount; deckId++)
        {
            foreach (var rank in allRanks)
            {
                foreach (var suit in allSuits)
                {
                    deck.Add(new Card { Rank = rank, Suit = suit, DeckId = deckId });
                }
            }
        }

        return deck;
    }

    private void ShuffleInPlace(List<Card> deck)
    {
        for (var i = deck.Count - 1; i > 0; i--)
        {
            var j = _random.Next(i + 1);
            (deck[i], deck[j]) = (deck[j], deck[i]);
        }
    }
}