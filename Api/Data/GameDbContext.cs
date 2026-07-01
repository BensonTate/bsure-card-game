using Api.Models;
using Microsoft.EntityFrameworkCore;

namespace Api.Data;

// EF Core DbContext for the card game.

public class GameDbContext : DbContext
{
    public GameDbContext(DbContextOptions<GameDbContext> options) : base(options)
    {
    }

    public DbSet<Game> Games => Set<Game>();
    public DbSet<Player> Players => Set<Player>();
    public DbSet<Card> Cards => Set<Card>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Game -> Players: one-to-many, cascade delete.
        modelBuilder.Entity<Game>()
            .HasMany(g => g.Players)
            .WithOne(p => p.Game)
            .HasForeignKey(p => p.GameId)
            .OnDelete(DeleteBehavior.Cascade);

        // Player -> Cards: one-to-many, cascade delete.
        modelBuilder.Entity<Player>()
            .HasMany(p => p.Cards)
            .WithOne(c => c.Player)
            .HasForeignKey(c => c.PlayerId)
            .OnDelete(DeleteBehavior.Cascade);

        // Index on CreatedAt for the paginated GET /games query.
        modelBuilder.Entity<Game>()
            .HasIndex(g => g.CreatedAt);
    }
}