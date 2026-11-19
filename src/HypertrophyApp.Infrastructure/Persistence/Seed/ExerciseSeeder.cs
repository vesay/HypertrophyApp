using System.Text.Json;
using System.Text.Json.Serialization;
using HypertrophyApp.Domain.Exercises;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace HypertrophyApp.Infrastructure.Persistence.Seed;

/// <summary>Loads the bundled public-domain exercise library into the database on first start.</summary>
public static class ExerciseSeeder
{
    private sealed record SourceExercise(
        string Id, string Name, string Category, string Level, string? Equipment, string? Force, string? Mechanic,
        string[] PrimaryMuscles, string[] SecondaryMuscles, string[] Instructions, string[] Images);

    private static readonly JsonSerializerOptions Json = new() { PropertyNameCaseInsensitive = true };

    public static async Task SeedAsync(HypertrophyAppDbContext db, ILogger logger, CancellationToken ct = default)
    {
        if (await db.Exercises.AnyAsync(ct)) return;

        await using var stream = typeof(ExerciseSeeder).Assembly
            .GetManifestResourceStream("HypertrophyApp.Infrastructure.Persistence.Seed.exercises.json")
            ?? throw new InvalidOperationException("Embedded exercises.json not found.");

        var source = await JsonSerializer.DeserializeAsync<List<SourceExercise>>(stream, Json, ct) ?? [];

        db.Exercises.AddRange(source
            .Where(s => !string.IsNullOrWhiteSpace(s.Id) && !string.IsNullOrWhiteSpace(s.Name))
            .DistinctBy(s => s.Id)
            .Select(s => Exercise.Create(
                s.Id, s.Name, s.Category, s.Level, s.Equipment, s.Force, s.Mechanic,
                s.PrimaryMuscles ?? [], s.SecondaryMuscles ?? [], s.Instructions ?? [], s.Images ?? [])));

        var count = await db.SaveChangesAsync(ct);
        logger.LogInformation("Seeded {Count} exercises into the exercise library.", count);
    }
}
