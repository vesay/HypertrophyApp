namespace HypertrophyApp.Shared.Programs;

/// <param name="DayOfWeek">English day name, e.g. "Monday".</param>
public sealed record CustomDayDto(string DayOfWeek, string Title, IReadOnlyList<ExerciseDto> Exercises);

public sealed record SaveCustomProgramRequest(IReadOnlyList<CustomDayDto> Days);
