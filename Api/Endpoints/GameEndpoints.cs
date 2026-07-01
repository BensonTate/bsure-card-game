using Api.Data;
using Api.Dtos;
using Api.Services;
using Microsoft.EntityFrameworkCore;

namespace Api.Endpoints;

public static class GameEndpoints
{
    // Pagination bounds — kept as constants for defence: "if a client asks for
    // 10,000 items per page, we cap it at 50 to protect the DB from accidental
    // full-table dumps." Sensible defaults for the frontend's default page load.
    private const int DefaultPageSize = 10;
    private const int MaxPageSize = 50;

    public static void MapGameEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/games");

        group.MapPost("/", CreateGame);
        group.MapGet("/{id:int}", GetGameById);
        group.MapGet("/", ListGames);
        group.MapPost("/{id:int}/redeal", RedealGame);   // NEW
    }

    private static async Task<IResult> CreateGame(
        IDealingService dealer,
        IScoringService scorer,
        GameDbContext db)
    {
        var game = dealer.DealNewGame();
        scorer.ScoreGame(game.Players);

        db.Games.Add(game);
        await db.SaveChangesAsync();

        return Results.Created($"/games/{game.Id}", GameMapper.ToDto(game));
    }

    private static async Task<IResult> GetGameById(int id, GameDbContext db)
    {
        var game = await db.Games
            .AsNoTracking()
            .Include(g => g.Players)
                .ThenInclude(p => p.Cards)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return Results.Problem(
                detail: $"Game with id {id} was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        return Results.Ok(GameMapper.ToDto(game));
    }

    // GET /games?page=1&pageSize=10 — paginated list of past games.
    // Returns a PagedResult envelope so the frontend can render "Page X of Y".
    // Ordered by CreatedAt DESC (most recent first) — the index on CreatedAt
    // in GameDbContext makes this cheap.
    private static async Task<IResult> ListGames(
        GameDbContext db,
        int page = 1,
        int pageSize = DefaultPageSize)
    {
        if (page < 1)
        {
            return Results.Problem(
                detail: "Query parameter 'page' must be 1 or greater.",
                statusCode: StatusCodes.Status400BadRequest);
        }
        if (pageSize < 1 || pageSize > MaxPageSize)
        {
            return Results.Problem(
                detail: $"Query parameter 'pageSize' must be between 1 and {MaxPageSize}.",
                statusCode: StatusCodes.Status400BadRequest);
        }

        var totalCount = await db.Games.CountAsync();
        var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);

        var games = await db.Games
            .AsNoTracking()
            .Include(g => g.Players)
            .OrderByDescending(g => g.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = games.Select(GameMapper.ToSummaryDto).ToList();

        return Results.Ok(new PagedResult<GameSummaryDto>(
            page, pageSize, totalCount, totalPages, items));
    }
    private static async Task<IResult> RedealGame(
        int id,
        IDealingService dealer,
        IScoringService scorer,
        GameDbContext db)
    {
        var game = await db.Games
            .Include(g => g.Players)
                .ThenInclude(p => p.Cards)
            .FirstOrDefaultAsync(g => g.Id == id);

        if (game is null)
        {
            return Results.Problem(
                detail: $"Game with id {id} was not found.",
                statusCode: StatusCodes.Status404NotFound);
        }

        dealer.RedealGame(game);
        scorer.ScoreGame(game.Players);

        await db.SaveChangesAsync();

        return Results.Ok(GameMapper.ToDto(game));
    }
}