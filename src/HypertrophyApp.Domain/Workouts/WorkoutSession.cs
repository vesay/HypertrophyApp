namespace HypertrophyApp.Domain.Workouts;

/// <summary>What the user actually did on a given day. One session per profile per date.</summary>
public sealed class WorkoutSession
{
    public const int MaxTitleLength = 60, MaxSets = 100;

    private WorkoutSession() { } // EF Core

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public string Title { get; private set; } = string.Empty;
    public List<SetLog> Sets { get; private set; } = [];
    public DateTime UpdatedAtUtc { get; private set; }

    public static WorkoutSession Create(Guid profileId, DateOnly date, string title, IEnumerable<SetLog> sets, DateTime nowUtc)
    {
        var session = new WorkoutSession { Id = Guid.NewGuid(), ProfileId = profileId, Date = date };
        session.Replace(title, sets, nowUtc);
        return session;
    }

    /// <summary>Overwrites the logged sets (the client always sends the full day).</summary>
    public void Replace(string title, IEnumerable<SetLog> sets, DateTime nowUtc)
    {
        title = title?.Trim() ?? string.Empty;
        var list = sets.ToList();

        if (title.Length is 0 or > MaxTitleLength)
            throw new ArgumentException($"Workout name must be 1-{MaxTitleLength} characters.", nameof(title));
        if (list.Count is 0 or > MaxSets)
            throw new ArgumentException($"Log between 1 and {MaxSets} sets.", nameof(sets));

        Title = title;
        Sets = list;
        UpdatedAtUtc = nowUtc;
    }

    public int TotalReps => Sets.Sum(s => s.Reps);
    public decimal TotalVolumeKg => Sets.Sum(s => s.Reps * (s.WeightKg ?? 0));
}

/// <summary>A single performed set.</summary>
public sealed class SetLog
{
    private SetLog() { } // EF Core

    public string ExerciseName { get; private set; } = string.Empty;
    public int SetNumber { get; private set; }
    public int Reps { get; private set; }
    public decimal? WeightKg { get; private set; }

    public static SetLog Create(string exerciseName, int setNumber, int reps, decimal? weightKg)
    {
        exerciseName = exerciseName?.Trim() ?? string.Empty;

        if (exerciseName.Length is 0 or > 80)
            throw new ArgumentException("Exercise name must be 1-80 characters.", nameof(exerciseName));
        if (setNumber is < 1 or > 20)
            throw new ArgumentOutOfRangeException(nameof(setNumber));
        if (reps is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(reps), $"{exerciseName}: reps must be between 0 and 1000.");
        if (weightKg is < 0 or > 1000)
            throw new ArgumentOutOfRangeException(nameof(weightKg), $"{exerciseName}: weight must be between 0 and 1000 kg.");

        return new SetLog { ExerciseName = exerciseName, SetNumber = setNumber, Reps = reps, WeightKg = weightKg };
    }

    /// <summary>Estimated one-rep max (Epley). Only meaningful for 1-12 reps with a load.</summary>
    public decimal? Estimated1RmKg =>
        WeightKg is > 0 && Reps is >= 1 and <= 12
            ? Math.Round(WeightKg.Value * (1 + Reps / 30m), 1)
            : null;
}
