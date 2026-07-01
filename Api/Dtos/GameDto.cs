namespace Api.Dtos;

public record GameDto(
    int Id,
    DateTime CreatedAt,
    IReadOnlyList<PlayerDto> Players);

public record PlayerDto(
    int Id,
    int SeatNumber,
    string Name,
    int HandScore,
    long? SuitProduct,      // Null when this player wasn't part of a tie-break.
    bool IsWinner,
    IReadOnlyList<CardDto> Cards);

public record CardDto(
    int Id,
    string Rank,            // Serialize as string ("Ace", "King") for frontend readability.
    string Suit,            // Same reasoning — the frontend renders these directly.
    int DeckId);