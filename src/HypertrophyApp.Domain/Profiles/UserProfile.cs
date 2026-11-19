namespace HypertrophyApp.Domain.Profiles;

/// <summary>
/// An anonymous user profile. By design HypertrophyApp stores no personal identifiers
/// (no e-mail, phone or password) – only what is needed to build a training and nutrition plan.
/// </summary>
public sealed class UserProfile
{
    public const int MinAge = 13, MaxAge = 90;
    public const int MinHeightCm = 120, MaxHeightCm = 230;
    public const int MinWeightKg = 30, MaxWeightKg = 300;
    public const int MinDaysPerWeek = 1, MaxDaysPerWeek = 6;
    public const int MaxNameLength = 50;

    /// <summary>Split id used when the user follows their own program instead of a built-in split.</summary>
    public const string CustomSplitId = "custom";

    private UserProfile() { } // EF Core

    public Guid Id { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public Sex Sex { get; private set; }
    public int Age { get; private set; }
    public decimal HeightCm { get; private set; }
    public decimal WeightKg { get; private set; }
    public int DaysPerWeek { get; private set; }
    public string SplitId { get; private set; } = string.Empty;
    public FitnessGoal Goal { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }
    public DateTime UpdatedAtUtc { get; private set; }

    /// <summary>The user's own program; kept even when they switch back to a built-in split.</summary>
    public List<CustomWorkoutDay> CustomProgram { get; private set; } = [];

    public bool UsesCustomProgram => SplitId == CustomSplitId;

    public static UserProfile Create(
        string name, Sex sex, int age, decimal heightCm, decimal weightKg,
        int daysPerWeek, string splitId, FitnessGoal goal, DateTime nowUtc)
    {
        var profile = new UserProfile { Id = Guid.NewGuid(), CreatedAtUtc = nowUtc };
        profile.Update(name, sex, age, heightCm, weightKg, daysPerWeek, splitId, goal, nowUtc);
        return profile;
    }

    public void Update(
        string name, Sex sex, int age, decimal heightCm, decimal weightKg,
        int daysPerWeek, string splitId, FitnessGoal goal, DateTime nowUtc)
    {
        name = name?.Trim() ?? string.Empty;

        if (name.Length is 0 or > MaxNameLength)
            throw new ArgumentException($"Name must be 1-{MaxNameLength} characters.", nameof(name));
        if (age is < MinAge or > MaxAge)
            throw new ArgumentOutOfRangeException(nameof(age), $"Age must be between {MinAge} and {MaxAge}.");
        if (heightCm < MinHeightCm || heightCm > MaxHeightCm)
            throw new ArgumentOutOfRangeException(nameof(heightCm), $"Height must be between {MinHeightCm} and {MaxHeightCm} cm.");
        if (weightKg < MinWeightKg || weightKg > MaxWeightKg)
            throw new ArgumentOutOfRangeException(nameof(weightKg), $"Weight must be between {MinWeightKg} and {MaxWeightKg} kg.");
        if (daysPerWeek is < MinDaysPerWeek or > MaxDaysPerWeek)
            throw new ArgumentOutOfRangeException(nameof(daysPerWeek), $"Training days must be between {MinDaysPerWeek} and {MaxDaysPerWeek}.");
        if (string.IsNullOrWhiteSpace(splitId))
            throw new ArgumentException("A training split must be selected.", nameof(splitId));
        if (splitId == CustomSplitId && CustomProgram.Count == 0)
            throw new ArgumentException("Create your own workout before selecting it.", nameof(splitId));
        if (!Enum.IsDefined(sex))
            throw new ArgumentOutOfRangeException(nameof(sex));
        if (!Enum.IsDefined(goal))
            throw new ArgumentOutOfRangeException(nameof(goal));

        Name = name;
        Sex = sex;
        Age = age;
        HeightCm = heightCm;
        WeightKg = weightKg;
        SplitId = splitId;
        // A custom program defines its own number of training days.
        DaysPerWeek = splitId == CustomSplitId ? CustomProgram.Count : daysPerWeek;
        Goal = goal;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Replaces the custom program and switches the profile to it.</summary>
    public void SetCustomProgram(IEnumerable<CustomWorkoutDay> days, DateTime nowUtc)
    {
        var list = days.ToList();

        if (list.Count is < MinDaysPerWeek or > MaxDaysPerWeek)
            throw new ArgumentException($"A program needs {MinDaysPerWeek}-{MaxDaysPerWeek} training days (keep at least one rest day).");
        if (list.Select(d => d.Day).Distinct().Count() != list.Count)
            throw new ArgumentException("Each day of the week can only have one workout.");

        CustomProgram = list.OrderBy(d => ((int)d.Day + 6) % 7).ToList(); // Monday first
        SplitId = CustomSplitId;
        DaysPerWeek = list.Count;
        UpdatedAtUtc = nowUtc;
    }

    /// <summary>Body Mass Index, kg/m².</summary>
    public decimal Bmi => Math.Round(WeightKg / (HeightCm / 100m * (HeightCm / 100m)), 1);
}
