namespace HypertrophyApp.Shared.Workouts;

public sealed record SetLogDto(string ExerciseName, int SetNumber, int Reps, decimal? WeightKg);

public sealed record SaveWorkoutSessionRequest(string Title, IReadOnlyList<SetLogDto> Sets);

public sealed record WorkoutSessionDto(DateOnly Date, string Title, IReadOnlyList<SetLogDto> Sets);

public sealed record WorkoutSessionSummaryDto(
    DateOnly Date, string Title, int Exercises, int TotalSets, int TotalReps, decimal TotalVolumeKg);

/// <summary>Best performance of one exercise on one day.</summary>
/// <param name="BestWeightKg">Heaviest load lifted that day (null for bodyweight exercises).</param>
/// <param name="BestReps">Most reps in a single set.</param>
/// <param name="Estimated1RmKg">Best estimated one-rep max (Epley), if a loaded set of 1-12 reps exists.</param>
public sealed record ExerciseProgressPointDto(
    DateOnly Date, decimal? BestWeightKg, int BestReps, int TotalReps, decimal VolumeKg, decimal? Estimated1RmKg);

public sealed record ExerciseProgressDto(string ExerciseName, IReadOnlyList<ExerciseProgressPointDto> Points);
