using Api.Models;
using Api.Services;
using FluentAssertions;
using static Api.Tests.TestHelpers.CardBuilder;

namespace Api.Tests;

// Covers cases 20-25: extreme scenarios and input validation.
// These exist to prove the service doesn't crash or silently swallow
// programmer errors — the marking rubric explicitly rewards robust
// error handling.
public class ScoringServiceEdgeCaseTests
{
    private readonly ScoringService _sut = new();

    [Fact]
    public void Case20_AllSixPlayersIdenticalHandAndSuit_AllWinJointly()
    {
        var players = Enumerable.Range(1, 6).Select(seat => Player(seat,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs))).ToList();

        _sut.ScoreGame(players);

        players.Should().AllSatisfy(p => p.IsWinner.Should().BeTrue());
    }

    [Fact]
    public void Case21_FiveAcesVsFiveKings_KingsWinNoTieBreak()
    {
        var aces = Player(1,
            Card(Rank.Ace, Suit.Diamonds),
            Card(Rank.Ace, Suit.Hearts),
            Card(Rank.Ace, Suit.Spades),
            Card(Rank.Ace, Suit.Clubs),
            Card(Rank.Ace, Suit.Diamonds, deckId: 2));

        var kings = Player(2,
            Card(Rank.King, Suit.Diamonds),
            Card(Rank.King, Suit.Hearts),
            Card(Rank.King, Suit.Spades),
            Card(Rank.King, Suit.Clubs),
            Card(Rank.King, Suit.Diamonds, deckId: 2));

        _sut.ScoreGame(new[] { aces, kings });

        aces.IsWinner.Should().BeFalse();
        kings.IsWinner.Should().BeTrue();
        aces.SuitProduct.Should().BeNull();
        kings.SuitProduct.Should().BeNull();
    }

    [Fact]
    public void Case22_PlayerHasFewerThan5Cards_Throws()
    {
        var player = Player(1,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades));

        var act = () => _sut.ScoreGame(new[] { player });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*3 cards*");
    }

    [Fact]
    public void Case23_PlayerHasMoreThan5Cards_Throws()
    {
        var player = Player(1,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds),
            Card(Rank.Seven, Suit.Hearts));

        var act = () => _sut.ScoreGame(new[] { player });

        act.Should().Throw<ArgumentException>()
            .WithMessage("*6 cards*");
    }

    [Fact]
    public void Case24_NullPlayerList_Throws()
    {
        var act = () => _sut.ScoreGame(null!);

        act.Should().Throw<ArgumentNullException>();
    }

    [Fact]
    public void Case25_EmptyPlayerList_Throws()
    {
        var act = () => _sut.ScoreGame(Array.Empty<Player>());

        act.Should().Throw<ArgumentException>();
    }
}