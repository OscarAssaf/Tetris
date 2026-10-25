using Microsoft.EntityFrameworkCore;

namespace Tetris.Api.Data;

public sealed class ScoreDbContext(DbContextOptions<ScoreDbContext> options) : DbContext(options)
{
    public DbSet<Score> Scores => Set<Score>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Score>(entity =>
        {
            entity.HasKey(score => score.Id);
            entity.Property(score => score.PlayerName).HasMaxLength(24).IsRequired();
            entity.HasIndex(score => new { score.Points, score.CreatedAtUtc });
        });
    }
}
