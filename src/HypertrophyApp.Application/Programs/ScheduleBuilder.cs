using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Shared.Programs;

namespace HypertrophyApp.Application.Programs;

/// <summary>Builds the weekly schedule for a profile from either a built-in split or the user's own program.</summary>
public static class ScheduleBuilder
{
    private static readonly DayOfWeek[] Week =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    public static SplitSummaryDto Split(UserProfile profile) =>
        profile.UsesCustomProgram
            ? new SplitSummaryDto(UserProfile.CustomSplitId, "My Program", "A workout plan you built yourself.",
                [profile.CustomProgram.Count], "Custom", "Your own split", IsRecommended: false)
            : ProgramCatalog.FindSplit(profile.SplitId, profile.DaysPerWeek)!;

    public static IReadOnlyList<ScheduleDayDto> Build(UserProfile profile)
    {
        if (!profile.UsesCustomProgram)
            return ProgramCatalog.BuildWeeklySchedule(profile.SplitId, profile.DaysPerWeek);

        return Week.Select(day =>
        {
            var custom = profile.CustomProgram.FirstOrDefault(d => d.Day == day);
            return new ScheduleDayDto(day.ToString(), custom is null ? null : ToWorkout(custom));
        }).ToList();
    }

    public static IReadOnlyList<CustomDayDto> ToDtos(IEnumerable<CustomWorkoutDay> days) =>
        days.Select(d => new CustomDayDto(d.Day.ToString(), d.Title, ToWorkout(d).Exercises)).ToList();

    private static WorkoutDayDto ToWorkout(CustomWorkoutDay day) => new(
        day.Title,
        "Custom workout",
        day.Exercises.Select(e => new ExerciseDto(e.Name, e.Sets, e.Reps, e.RestSeconds, e.ExerciseId)).ToList());
}
