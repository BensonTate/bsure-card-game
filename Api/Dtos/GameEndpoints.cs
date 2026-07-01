using Api.Data;
using Api.Dtos;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class GameEndpoints
{
    public static void MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        group.MapPost("/", CreateGame);
        // GET /games/{id}, GET /games, POST /games/{id}/redeal come next.
    }

    // POST /games — deal a new game, score it, persist it, return the full result.
    // Returns 201 Created with a Location header pointing to GET /games/{id}.
    private static async Task<IResult> CreateGame(
        IDealingService dealer,
        IScoringService scorer,
        GameDbContext db)
    {
       
        var game = dealer.DealNewGame();

        scorer.ScoreGame(game.Players);

        db.Games.Add(game);
        await db.SaveChangesAsync();

        // 201 Created with Location header
        return Results.Created($"/games/{game.Id}", GameMapper.ToDto(game));
    }
}