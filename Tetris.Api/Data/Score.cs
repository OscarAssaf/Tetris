namespace Tetris.Api.Data;

public sealed class Score
{
    public int Id { get; set; }
    public required string PlayerName { get; set; }
    public int Points { get; set; }
    public DateTime CreatedAtUtc { get; set; }
}
