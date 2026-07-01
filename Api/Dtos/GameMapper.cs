using Api.Models;

namespace Api.Dtos;

public static class GameMapper
{

    public static GameDto ToDto(Game game) => new(
        game.Id,
        game.CreatedAt,
        game.Players
            .OrderBy(p => p.SeatNumber)
            .Select(ToDto)
            .ToList());

    private static PlayerDto ToDto(Player player) => new(
        player.Id,
        player.SeatNumber,
        player.Name,
        player.HandScore,
        player.SuitProduct,
        player.IsWinner,
        player.Cards.Select(ToDto).ToList());

    private static CardDto ToDto(Card card) => new(
        card.Id,
        card.Rank.ToString(),
        card.Suit.ToString(),
        card.DeckId);

    public static GameSummaryDto ToSummaryDto(Game game)
    {
        var winners = game.Players.Where(p => p.IsWinner).ToList();
        var topScore = game.Players.Any() ? game.Players.Max(p => p.HandScore) : 0;

        return new GameSummaryDto(
            game.Id,
            game.CreatedAt,
            winners.Count,
            topScore,
            winners.Select(w => w.Name).ToList());
    }
}