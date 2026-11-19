namespace HypertrophyApp.Domain.Exercises;

/// <summary>An entry of the exercise library (name, muscles, equipment, form instructions, demo images).</summary>
public sealed class Exercise
{
    private Exercise() { } // EF Core

    /// <summary>Stable id from the source dataset, e.g. "Barbell_Squat".</summary>
    public string Id { get; private set; } = string.Empty;
    public string Name { get; private set; } = string.Empty;
    public string Category { get; private set; } = string.Empty;
    public string Level { get; private set; } = string.Empty;
    public string? Equipment { get; private set; }
    public string? Force { get; private set; }
    public string? Mechanic { get; private set; }
    public string[] PrimaryMuscles { get; private set; } = [];
    public string[] SecondaryMuscles { get; private set; } = [];
    public string[] Instructions { get; private set; } = [];

    /// <summary>Relative image paths (start and end position), resolved against the configured media base URL.</summary>
    public string[] ImagePaths { get; private set; } = [];

    public static Exercise Create(
        string id, string name, string category, string level, string? equipment, string? force, string? mechanic,
        IEnumerable<string> primaryMuscles, IEnumerable<string> secondaryMuscles,
        IEnumerable<string> instructions, IEnumerable<string> imagePaths)
    {
        if (string.IsNullOrWhiteSpace(id)) throw new ArgumentException("Exercise id is required.", nameof(id));
        if (string.IsNullOrWhiteSpace(name)) throw new ArgumentException("Exercise name is required.", nameof(name));

        return new Exercise
        {
            Id = id,
            Name = name.Trim(),
            Category = category,
            Level = level,
            Equipment = equipment,
            Force = force,
            Mechanic = mechanic,
            PrimaryMuscles = primaryMuscles.ToArray(),
            SecondaryMuscles = secondaryMuscles.ToArray(),
            Instructions = instructions.ToArray(),
            ImagePaths = imagePaths.ToArray()
        };
    }
}
