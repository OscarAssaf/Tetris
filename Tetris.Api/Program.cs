using Microsoft.EntityFrameworkCore;
using Tetris.Api.Data;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ScoreDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("Scores")));

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>() ?? [];
builder.Services.AddCors(options => options.AddDefaultPolicy(policy =>
    policy.WithOrigins(allowedOrigins).AllowAnyHeader().AllowAnyMethod()));

var app = builder.Build();
app.UseCors();

await using (var scope = app.Services.CreateAsyncScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ScoreDbContext>();
    await db.Database.EnsureCreatedAsync();
}

app.MapGet("/api/scores", async (ScoreDbContext db, CancellationToken cancellationToken) =>
{
    var scores = await db.Scores
        .AsNoTracking()
        .OrderByDescending(score => score.Points)
        .ThenBy(score => score.CreatedAtUtc)
        .Take(10)
        .Select(score => new ScoreResponse(score.Id, score.PlayerName, score.Points, score.CreatedAtUtc))
        .ToListAsync(cancellationToken);

    return Results.Ok(scores);
});

app.MapPost("/api/scores", async (SubmitScoreRequest request, ScoreDbContext db, CancellationToken cancellationToken) =>
{
    var name = request.PlayerName?.Trim();
    if (string.IsNullOrWhiteSpace(name) || name.Length > 24)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["playerName"] = ["Enter a name between 1 and 24 characters."] });
    if (request.Points < 0 || request.Points > 10_000_000)
        return Results.ValidationProblem(new Dictionary<string, string[]> { ["points"] = ["Score must be between 0 and 10,000,000."] });

    var score = new Score { PlayerName = name, Points = request.Points, CreatedAtUtc = DateTime.UtcNow };
    db.Scores.Add(score);
    await db.SaveChangesAsync(cancellationToken);

    return Results.Created($"/api/scores/{score.Id}", new ScoreResponse(score.Id, score.PlayerName, score.Points, score.CreatedAtUtc));
});

app.Run();

public sealed record SubmitScoreRequest(string? PlayerName, int Points);
public sealed record ScoreResponse(int Id, string PlayerName, int Points, DateTime CreatedAtUtc);
