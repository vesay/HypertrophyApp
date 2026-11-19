namespace HypertrophyApp.Shared.Exercises;

/// <summary>Compact library entry for search results and pickers.</summary>
public sealed record ExerciseSummaryDto(
    string Id, string Name, string Level, string? Equipment,
    IReadOnlyList<string> PrimaryMuscles, IReadOnlyList<string> ImageUrls);

public sealed record ExerciseDetailDto(
    string Id, string Name, string Category, string Level, string? Equipment, string? Force, string? Mechanic,
    IReadOnlyList<string> PrimaryMuscles, IReadOnlyList<string> SecondaryMuscles,
    IReadOnlyList<string> Instructions, IReadOnlyList<string> ImageUrls);

public sealed record ExerciseFiltersDto(IReadOnlyList<string> Muscles, IReadOnlyList<string> Equipment);

public sealed record ExerciseSearchResultDto(IReadOnlyList<ExerciseSummaryDto> Items, int Total);
