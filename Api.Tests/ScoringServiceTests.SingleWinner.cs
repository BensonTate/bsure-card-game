using Api.Models;
using Api.Services;
using FluentAssertions;
using static Api.Tests.TestHelpers.CardBuilder;

namespace Api.Tests;

// Covers cases 12-13: when one player has a strictly highest hand score,
// they win outright and no suit-product calculation happens for anyone.
public class ScoringServiceSingleWinnerTests
{
    private readonly ScoringService _sut = new();

    [Fact]
    public void Case12_OnePlayerHasHighestScore_IsSoleWinner()
    {
        var winner = Player(1,
            Card(Rank.King, Suit.Spades),
            Card(Rank.King, Suit.Hearts),
            Card(Rank.King, Suit.Clubs),
            Card(Rank.King, Suit.Diamonds),
            Card(Rank.King, Suit.Spades, deckId: 2));   // 65

        var loser = Player(2,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds));             // 20

        _sut.ScoreGame(new[] { winner, loser });

        winner.IsWinner.Should().BeTrue();
        loser.IsWinner.Should().BeFalse();
    }

    [Fact]
    public void Case13_NoTie_SuitProductIsNullForAllPlayers()
    {
        // SuitProduct = null is the signal the frontend uses to hide the
        // suit-score column. 
        var winner = Player(1,
            Card(Rank.King, Suit.Spades),
            Card(Rank.King, Suit.Hearts),
            Card(Rank.King, Suit.Clubs),
            Card(Rank.King, Suit.Diamonds),
            Card(Rank.King, Suit.Spades, deckId: 2));

        var loser = Player(2,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds));

        _sut.ScoreGame(new[] { winner, loser });

        winner.SuitProduct.Should().BeNull();
        loser.SuitProduct.Should().BeNull();
    }
}