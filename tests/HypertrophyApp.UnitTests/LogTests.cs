using HypertrophyApp.Domain.Nutrition;
using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Domain.Workouts;

namespace HypertrophyApp.UnitTests;

public class WorkoutSessionTests
{
    private static readonly DateOnly Day = new(2026, 11, 16);

    [Fact]
    public void Totals_are_calculated_from_sets()
    {
        var session = WorkoutSession.Create(Guid.NewGuid(), Day, "Push",
            [SetLog.Create("Bench", 1, 8, 60), SetLog.Create("Bench", 2, 6, 65), SetLog.Create("Push-Up", 1, 20, null)],
            DateTime.UtcNow);

        Assert.Equal(34, session.TotalReps);
        Assert.Equal(8 * 60 + 6 * 65, session.TotalVolumeKg);
    }

    [Fact]
    public void Replace_overwrites_previous_sets()
    {
        var session = WorkoutSession.Create(Guid.NewGuid(), Day, "Push", [SetLog.Create("Bench", 1, 8, 60)], DateTime.UtcNow);
        session.Replace("Pull", [SetLog.Create("Row", 1, 10, 50), SetLog.Create("Row", 2, 10, 50)], DateTime.UtcNow);

        Assert.Equal("Pull", session.Title);
        Assert.Equal(2, session.Sets.Count);
        Assert.All(session.Sets, s => Assert.Equal("Row", s.ExerciseName));
    }

    [Fact]
    public void Empty_session_is_rejected() =>
        Assert.Throws<ArgumentException>(() => WorkoutSession.Create(Guid.NewGuid(), Day, "Push", [], DateTime.UtcNow));

    [Theory]
    [InlineData(5, 100, 116.7)]  // Epley: 100 * (1 + 5/30)
    [InlineData(1, 100, 103.3)]
    public void Estimated_one_rep_max_uses_epley(int reps, double weight, double expected) =>
        Assert.Equal((decimal)expected, SetLog.Create("Squat", 1, reps, (decimal)weight).Estimated1RmKg);

    [Fact]
    public void Estimated_one_rep_max_is_skipped_for_high_reps_and_bodyweight()
    {
        Assert.Null(SetLog.Create("Squat", 1, 20, 60).Estimated1RmKg);
        Assert.Null(SetLog.Create("Pull-Up", 1, 8, null).Estimated1RmKg);
    }

    [Theory]
    [InlineData(-1, 50)]
    [InlineData(10, -5)]
    [InlineData(10, 2000)]
    public void Invalid_sets_are_rejected(int reps, double weight) =>
        Assert.ThrowsAny<ArgumentException>(() => SetLog.Create("Bench", 1, reps, (decimal)weight));
}

public class FoodEntryTests
{
    [Fact]
    public void Valid_entry_is_created_with_trimmed_name()
    {
        var entry = FoodEntry.Create(Guid.NewGuid(), new DateOnly(2026, 11, 16), MealType.Lunch, "  Chicken & rice ", 650, 45, 70, 15, DateTime.UtcNow);
        Assert.Equal("Chicken & rice", entry.Name);
        Assert.Equal(650, entry.Calories);
    }

    [Theory]
    [InlineData("", 100)]
    [InlineData("Pizza", -1)]
    [InlineData("Pizza", 20_000)]
    public void Invalid_entries_are_rejected(string name, int calories) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            FoodEntry.Create(Guid.NewGuid(), new DateOnly(2026, 11, 16), MealType.Dinner, name, calories, null, null, null, DateTime.UtcNow));
}

public class CustomProgramTests
{
    private static UserProfile NewProfile() =>
        UserProfile.Create("Alex", Sex.Male, 27, 178, 76, 3, "full-body", FitnessGoal.Maintain, DateTime.UtcNow);

    private static CustomWorkoutDay Day(DayOfWeek day) =>
        CustomWorkoutDay.Create(day, "Workout", [CustomExercise.Create("Squat", 3, "5", 120)]);

    [Fact]
    public void Saving_a_custom_program_switches_to_it_and_sets_days_per_week()
    {
        var profile = NewProfile();
        profile.SetCustomProgram([Day(DayOfWeek.Friday), Day(DayOfWeek.Monday)], DateTime.UtcNow);

        Assert.True(profile.UsesCustomProgram);
        Assert.Equal(2, profile.DaysPerWeek);
        Assert.Equal(DayOfWeek.Monday, profile.CustomProgram[0].Day); // ordered Monday first
    }

    [Fact]
    public void Switching_back_to_a_template_keeps_the_custom_program()
    {
        var profile = NewProfile();
        profile.SetCustomProgram([Day(DayOfWeek.Monday)], DateTime.UtcNow);
        profile.Update("Alex", Sex.Male, 27, 178, 76, 4, "upper-lower", FitnessGoal.Maintain, DateTime.UtcNow);

        Assert.False(profile.UsesCustomProgram);
        Assert.Equal(4, profile.DaysPerWeek);
        Assert.Single(profile.CustomProgram);
    }

    [Fact]
    public void Custom_split_cannot_be_selected_without_a_program() =>
        Assert.Throws<ArgumentException>(() =>
            NewProfile().Update("Alex", Sex.Male, 27, 178, 76, 3, UserProfile.CustomSplitId, FitnessGoal.Maintain, DateTime.UtcNow));

    [Fact]
    public void Seven_training_days_or_duplicate_days_are_rejected()
    {
        var profile = NewProfile();
        Assert.Throws<ArgumentException>(() => profile.SetCustomProgram(Enum.GetValues<DayOfWeek>().Select(Day), DateTime.UtcNow));
        Assert.Throws<ArgumentException>(() => profile.SetCustomProgram([Day(DayOfWeek.Monday), Day(DayOfWeek.Monday)], DateTime.UtcNow));
    }
}
