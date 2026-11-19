namespace HypertrophyApp.Shared.Programs;

/// <summary>A training split the user can pick during onboarding.</summary>
/// <param name="TimesPerWeekPerMuscle">How often each muscle group is trained per week with this split.</param>
/// <param name="IsRecommended">Whether this is the evidence-based default for the requested number of days.</param>
public sealed record SplitSummaryDto(
    string Id, string Name, string Description, IReadOnlyList<int> SupportedDays, string Level,
    string TimesPerWeekPerMuscle, bool IsRecommended);

/// <param name="ExerciseId">Link to the exercise library (null for free-text exercises).</param>
/// <param name="ImageUrls">Start/end position images from the library, filled in by the server.</param>
public sealed record ExerciseDto(
    string Name, int Sets, string Reps, int RestSeconds,
    string? ExerciseId = null, IReadOnlyList<string>? ImageUrls = null);

public sealed record WorkoutDayDto(string Title, string Focus, IReadOnlyList<ExerciseDto> Exercises);

/// <summary>One calendar day of the week: either a workout or a rest day.</summary>
public sealed record ScheduleDayDto(string DayOfWeek, WorkoutDayDto? Workout)
{
    public bool IsRestDay => Workout is null;
}
