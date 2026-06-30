using Api.Models;
using Api.Services;
using FluentAssertions;
using static Api.Tests.TestHelpers.CardBuilder;

namespace Api.Tests;

// Covers cases 1-7 from the ticket: hand score calculation under the
// game's value rules. These tests pin down the most fundamental rule
// of the scoring system: Ace = 11 (not 1), and face cards have specific values.
public class ScoringServiceHandScoreTests
{
    private readonly ScoringService _sut = new();

    [Fact]
    public void Case1_AllNumberCards_SumsFaceValues()
    {
        var player = Player(1,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Three, Suit.Hearts),
            Card(Rank.Four, Suit.Spades),
            Card(Rank.Five, Suit.Clubs),
            Card(Rank.Six, Suit.Diamonds));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(20);
    }

    [Fact]
    public void Case2_MixedFaceAndNumber_SumsCorrectly()
    {
        var player = Player(1,
            Card(Rank.King, Suit.Spades),
            Card(Rank.Ten, Suit.Diamonds),
            Card(Rank.Five, Suit.Hearts),
            Card(Rank.Three, Suit.Clubs),
            Card(Rank.Two, Suit.Spades));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(33);
    }

    [Fact]
    public void Case3_AllAces_CountAs11Each_NotAs1()
    {
        var player = Player(1,
            Card(Rank.Ace, Suit.Diamonds),
            Card(Rank.Ace, Suit.Hearts),
            Card(Rank.Ace, Suit.Spades),
            Card(Rank.Ace, Suit.Clubs),
            Card(Rank.Ace, Suit.Diamonds, deckId: 2));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(55);
    }

    [Fact]
    public void Case4_AllFaceCards_SumsToCorrectValue()
    {
        var player = Player(1,
            Card(Rank.Jack, Suit.Diamonds),
            Card(Rank.Queen, Suit.Hearts),
            Card(Rank.King, Suit.Spades),
            Card(Rank.Jack, Suit.Clubs),
            Card(Rank.Queen, Suit.Diamonds));

        _sut.ScoreGame(new[] { player });

        // J(11) + Q(12) + K(13) + J(11) + Q(12) = 59
        player.HandScore.Should().Be(59);
    }

    [Fact]
    public void Case5_DuplicateRankAcrossDecks_IsValid()
    {
        // Two decks => the same (rank, suit) can appear twice, distinguished
        // only by DeckId. 
        var player = Player(1,
            Card(Rank.Seven, Suit.Hearts, deckId: 1),
            Card(Rank.Seven, Suit.Hearts, deckId: 2),
            Card(Rank.Seven, Suit.Spades),
            Card(Rank.Seven, Suit.Clubs),
            Card(Rank.Seven, Suit.Diamonds));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(35);
    }

    [Fact]
    public void Case6_FiveKings_GivesMaximumHand()
    {
        var player = Player(1,
            Card(Rank.King, Suit.Diamonds),
            Card(Rank.King, Suit.Hearts),
            Card(Rank.King, Suit.Spades),
            Card(Rank.King, Suit.Clubs),
            Card(Rank.King, Suit.Diamonds, deckId: 2));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(65);
    }

    [Fact]
    public void Case7_FiveTwos_GivesMinimumHand()
    {
        var player = Player(1,
            Card(Rank.Two, Suit.Diamonds),
            Card(Rank.Two, Suit.Hearts),
            Card(Rank.Two, Suit.Spades),
            Card(Rank.Two, Suit.Clubs),
            Card(Rank.Two, Suit.Diamonds, deckId: 2));

        _sut.ScoreGame(new[] { player });

        player.HandScore.Should().Be(10);
    }
}