using Api.Models;
using Api.Services;
using FluentAssertions;
using static Api.Tests.TestHelpers.CardBuilder;

namespace Api.Tests;

// Covers cases 14-19: tie on hand score, resolved (or not) by suit product.
// This is where the most interesting logic lives and where the interviewer
// is most likely to probe — be ready to walk through any of these by name.
public class ScoringServiceTieBreakTests
{
    private readonly ScoringService _sut = new();

    [Fact]
    public void Case14_TwoPlayersTieOnHandScore_HigherSuitProductWins()
    {
        var a = Player(1,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));

        var b = Player(2,
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds));

        _sut.ScoreGame(new[] { a, b });

        a.IsWinner.Should().BeTrue();
        b.IsWinner.Should().BeFalse();
        a.SuitProduct.Should().Be(1024);
        b.SuitProduct.Should().Be(1);
    }

    [Fact]
    public void Case15_ThreePlayersTie_OneHasHigherSuitProduct_WinsAlone()
    {
        var a = Player(1,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));      // product 1024

        var b = Player(2,
            Card(Rank.Ten, Suit.Hearts),
            Card(Rank.Ten, Suit.Hearts, deckId: 2),
            Card(Rank.Ten, Suit.Hearts),
            Card(Rank.Ten, Suit.Hearts, deckId: 2),
            Card(Rank.Ten, Suit.Hearts));     // product 32

        var c = Player(3,
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds));   // product 1

        _sut.ScoreGame(new[] { a, b, c });

        a.IsWinner.Should().BeTrue();
        b.IsWinner.Should().BeFalse();
        c.IsWinner.Should().BeFalse();
    }

    [Fact]
    public void Case16_SuitProductCalculatedOnlyForTiedPlayers()
    {
        var tiedA = Player(1,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));

        var tiedB = Player(2,
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds));

        var nonTied = Player(3,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds));

        _sut.ScoreGame(new[] { tiedA, tiedB, nonTied });

        tiedA.SuitProduct.Should().NotBeNull();
        tiedB.SuitProduct.Should().NotBeNull();
        nonTied.SuitProduct.Should().BeNull();
    }

    [Fact]
    public void Case17_TwoPlayersTieOnHandAndSuit_BothWinJointly()
    {
        
        var a = Player(1,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));

        var b = Player(2,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));

        _sut.ScoreGame(new[] { a, b });

        a.IsWinner.Should().BeTrue();
        b.IsWinner.Should().BeTrue();
    }

    [Fact]
    public void Case18_ThreePlayersTieOnHandAndSuit_AllThreeWinJointly()
    {
        var a = Player(1,
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades));

        var b = Player(2,
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades));

        var c = Player(3,
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades),
            Card(Rank.Ten, Suit.Spades, deckId: 2),
            Card(Rank.Ten, Suit.Spades));

        _sut.ScoreGame(new[] { a, b, c });

        a.IsWinner.Should().BeTrue();
        b.IsWinner.Should().BeTrue();
        c.IsWinner.Should().BeTrue();
    }

    [Fact]
    public void Case19_TieAtTop_SeparateTieLower_OnlyTopTieIsTieBroken()
    {
        
        var topA = Player(1,
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs),
            Card(Rank.Ten, Suit.Clubs, deckId: 2),
            Card(Rank.Ten, Suit.Clubs));     // 50

        var topB = Player(2,
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Ten, Suit.Diamonds, deckId: 2),
            Card(Rank.Ten, Suit.Diamonds));  // 50

        var lowA = Player(3,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds));   // 20

        var lowB = Player(4,
            Card(Rank.Two, Suit.Spades),
            Card(Rank.Three, Suit.Clubs),
            Card(Rank.Four, Suit.Diamonds),
            Card(Rank.Five, Suit.Hearts),
            Card(Rank.Six, Suit.Spades));     // 20

        _sut.ScoreGame(new[] { topA, topB, lowA, lowB });

        topA.SuitProduct.Should().NotBeNull();
        topB.SuitProduct.Should().NotBeNull();
        lowA.SuitProduct.Should().BeNull();
        lowB.SuitProduct.Should().BeNull();
        topA.IsWinner.Should().BeTrue();   // Clubs beats Diamonds.
        topB.IsWinner.Should().BeFalse();
        lowA.IsWinner.Should().BeFalse();
        lowB.IsWinner.Should().BeFalse();
    }
}