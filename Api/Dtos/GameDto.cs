namespace Api.Dtos;

// Existing records — unchanged.
public record GameDto(
    int Id,
    DateTime CreatedAt,
    IReadOnlyList<PlayerDto> Players);

public record PlayerDto(
    int Id,
    int SeatNumber,
    string Name,
    int HandScore,
    long? SuitProduct,
    bool IsWinner,
    IReadOnlyList<CardDto> Cards);

public record CardDto(
    int Id,
    string Rank,
    string Suit,
    int DeckId);

public record GameSummaryDto(
    int Id,
    DateTime CreatedAt,
    int WinnerCount,
    int TopHandScore,
    IReadOnlyList<string> WinnerNames);

public record PagedResult<T>(
    int Page,
    int PageSize,
    int TotalCount,
    int TotalPages,
    IReadOnlyList<T> Items);