namespace HypertrophyApp.Domain.Profiles;

/// <summary>One training day of a user-built program.</summary>
public sealed class CustomWorkoutDay
{
    public const int MaxTitleLength = 60, MaxExercises = 15;

    private CustomWorkoutDay() { } // EF Core

    public DayOfWeek Day { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public List<CustomExercise> Exercises { get; private set; } = [];

    public static CustomWorkoutDay Create(DayOfWeek day, string title, IEnumerable<CustomExercise> exercises)
    {
        title = title?.Trim() ?? string.Empty;
        var list = exercises.ToList();

        if (!Enum.IsDefined(day))
            throw new ArgumentOutOfRangeException(nameof(day));
        if (title.Length is 0 or > MaxTitleLength)
            throw new ArgumentException($"Workout name must be 1-{MaxTitleLength} characters.", nameof(title));
        if (list.Count is 0 or > MaxExercises)
            throw new ArgumentException($"{day}: add 1-{MaxExercises} exercises.", nameof(exercises));

        return new CustomWorkoutDay { Day = day, Title = title, Exercises = list };
    }
}

public sealed class CustomExercise
{
    public const int MaxNameLength = 80;

    private CustomExercise() { } // EF Core

    public string Name { get; private set; } = string.Empty;
    public int Sets { get; private set; }
    public string Reps { get; private set; } = string.Empty;
    public int RestSeconds { get; private set; }

    /// <summary>Exercise library id, or null for a free-text exercise.</summary>
    public string? ExerciseId { get; private set; }

    public static CustomExercise Create(string name, int sets, string reps, int restSeconds, string? exerciseId = null)
    {
        name = name?.Trim() ?? string.Empty;
        reps = reps?.Trim() ?? string.Empty;

        if (name.Length is 0 or > MaxNameLength)
            throw new ArgumentException($"Exercise name must be 1-{MaxNameLength} characters.", nameof(name));
        if (sets is < 1 or > 10)
            throw new ArgumentOutOfRangeException(nameof(sets), $"{name}: sets must be between 1 and 10.");
        if (reps.Length is 0 or > 20)
            throw new ArgumentException($"{name}: enter reps, e.g. \"8-12\".", nameof(reps));
        if (restSeconds is < 0 or > 600)
            throw new ArgumentOutOfRangeException(nameof(restSeconds), $"{name}: rest must be 0-600 seconds.");

        return new CustomExercise
        {
            Name = name, Sets = sets, Reps = reps, RestSeconds = restSeconds,
            ExerciseId = string.IsNullOrWhiteSpace(exerciseId) ? null : exerciseId
        };
    }
}
