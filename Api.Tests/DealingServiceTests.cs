using Api.Models;
using Api.Services;
using Api.Tests.TestHelpers;
using FluentAssertions;

namespace Api.Tests;

// Covers ticket cases 26-30: correctness of the deal itself.
// Scoring is NOT under test here — these tests only verify what came
// out of the deck and into players' hands. Keeping the two services
// (and their tests) separate is the whole point of splitting them.
public class DealingServiceTests
{
    // Helper to build a service with a fixed seed for repeatable tests.
    // Different tests use different seeds where the specific shuffle matters.
    private static DealingService CreateService(int seed = 42) =>
        new(new SeededRandomProvider(seed));

    [Fact]
    public void Case26_Deal_Produces6PlayersWith5CardsEach()
    {
        var sut = CreateService();

        var game = sut.DealNewGame();

        game.Players.Should().HaveCount(6);
        game.Players.Should().AllSatisfy(p => p.Cards.Should().HaveCount(5));

        // Total of 30 cards in play across all players.
        game.Players.SelectMany(p => p.Cards).Should().HaveCount(30);
    }

    [Fact]
    public void Case27_WithinASingleDeck_NoCardIsDealtTwice()
    {
        var sut = CreateService();

        var game = sut.DealNewGame();
        var dealtCards = game.Players.SelectMany(p => p.Cards).ToList();

        var duplicates = dealtCards
            .GroupBy(c => new { c.Rank, c.Suit, c.DeckId })
            .Where(g => g.Count() > 1)
            .ToList();

        duplicates.Should().BeEmpty(
            "no exact card identity should appear twice — that would mean a deck was drawn from with replacement");
    }

    [Fact]
    public void Case28_AcrossBothDecks_SameRankAndSuitMayAppearTwice()
    {
        DealingService sut;
        List<Card> dealtCards = new();
        IGrouping<(Rank, Suit), Card>? duplicateGroup = null;

        for (var seed = 1; seed <= 50 && duplicateGroup is null; seed++)
        {
            sut = CreateService(seed);
            dealtCards = sut.DealNewGame()
                .Players.SelectMany(p => p.Cards).ToList();

            duplicateGroup = dealtCards
                .GroupBy(c => (c.Rank, c.Suit))
                .FirstOrDefault(g => g.Count() > 1);
        }

        duplicateGroup.Should().NotBeNull(
            "with 30 cards drawn from 104 across 50 seeds, at least one shared rank+suit is overwhelmingly likely");

        // The duplicates must come from different decks — that's the whole
        // point of the two-deck rule.
        duplicateGroup!.Select(c => c.DeckId).Distinct().Should().HaveCountGreaterThan(1);
    }

    [Fact]
    public void Case29_SameSeed_ProducesSameDeal()
    {
        var first = CreateService(seed: 12345).DealNewGame();
        var second = CreateService(seed: 12345).DealNewGame();

        var firstCards = first.Players
            .SelectMany(p => p.Cards.Select(c => (p.SeatNumber, c.Rank, c.Suit, c.DeckId)))
            .ToList();

        var secondCards = second.Players
            .SelectMany(p => p.Cards.Select(c => (p.SeatNumber, c.Rank, c.Suit, c.DeckId)))
            .ToList();

        secondCards.Should().Equal(firstCards);
    }

    [Fact]
    public void Case30_TypicalDeal_DrawsFromBothDecks()
    {
        // 30 cards drawn from 104 will almost always touch both decks.
        var sut = CreateService();

        var game = sut.DealNewGame();
        var deckIds = game.Players
            .SelectMany(p => p.Cards)
            .Select(c => c.DeckId)
            .Distinct()
            .ToList();

        deckIds.Should().Contain(new[] { 1, 2 });
    }

    [Fact]
    public void Redeal_ReplacesAllCardsAndResetsScoringState()
    {
        var sut = CreateService(seed: 1);
        var game = sut.DealNewGame();

        // Pre-populate scoring state to prove the redeal clears it.
        foreach (var player in game.Players)
        {
            player.HandScore = 999;
            player.SuitProduct = 999;
            player.IsWinner = true;
        }

        var originalSeats = game.Players.Select(p => p.SeatNumber).ToList();

        var redealer = CreateService(seed: 2);
        redealer.RedealGame(game);

        game.Players.Select(p => p.SeatNumber).Should().Equal(originalSeats);
        game.Players.Should().AllSatisfy(p =>
        {
            p.Cards.Should().HaveCount(5);
            p.HandScore.Should().Be(0);
            p.SuitProduct.Should().BeNull();
            p.IsWinner.Should().BeFalse();
        });
    }

    [Fact]
    public void Redeal_NullGame_Throws()
    {
        var sut = CreateService();

        var act = () => sut.RedealGame(null!);

        act.Should().Throw<ArgumentNullException>();
    }
}