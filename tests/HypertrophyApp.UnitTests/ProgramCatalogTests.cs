using HypertrophyApp.Application.Programs;

namespace HypertrophyApp.UnitTests;

public class ProgramCatalogTests
{
    [Theory]
    [InlineData(1, "full-body-minimal")]
    [InlineData(2, "full-body")]
    [InlineData(3, "full-body")]
    [InlineData(4, "upper-lower")]
    [InlineData(5, "upper-lower-ppl")]
    [InlineData(6, "push-pull-legs")]
    public void Each_day_count_has_exactly_one_recommended_split_listed_first(int days, string expectedId)
    {
        var splits = ProgramCatalog.GetSplits(days);

        Assert.Single(splits, s => s.IsRecommended);
        Assert.Equal(expectedId, splits[0].Id);
        Assert.True(splits[0].IsRecommended);
        Assert.Equal(expectedId, ProgramCatalog.RecommendedSplitId(days));
    }

    [Theory]
    [InlineData(2)]
    [InlineData(3)]
    [InlineData(4)]
    [InlineData(5)]
    [InlineData(6)]
    public void Recommended_splits_train_each_muscle_at_least_twice_a_week_from_two_days(int days)
    {
        var id = ProgramCatalog.RecommendedSplitId(days)!;
        Assert.True(ProgramCatalog.TimesPerWeekPerMuscle(id, days) >= 2);
    }

    [Fact]
    public void All_supported_combinations_produce_a_full_week_with_the_right_number_of_workouts()
    {
        foreach (var split in ProgramCatalog.GetSplits())
        foreach (var days in split.SupportedDays)
        {
            var week = ProgramCatalog.BuildWeeklySchedule(split.Id, days);

            Assert.Equal(7, week.Count);
            Assert.Equal(days, week.Count(d => !d.IsRestDay));
            Assert.True(week[6].IsRestDay, "Sunday should always be a rest day.");
        }
    }

    [Fact]
    public void Push_pull_legs_six_days_runs_the_rotation_twice()
    {
        var titles = ProgramCatalog.BuildWeeklySchedule("push-pull-legs", 6)
            .Where(d => d.Workout is not null)
            .Select(d => d.Workout!.Title);

        Assert.Equal(["Push", "Pull", "Legs", "Push", "Pull", "Legs"], titles);
    }

    [Fact]
    public void Single_weekly_session_has_enough_sets_per_session()
    {
        var workout = ProgramCatalog.BuildWeeklySchedule("full-body-minimal", 1).Single(d => !d.IsRestDay).Workout!;
        Assert.True(workout.Exercises.Sum(e => e.Sets) >= 16);
    }

    [Fact]
    public void Unsupported_day_count_is_rejected()
    {
        Assert.False(ProgramCatalog.Supports("bro-split", 3));
        Assert.Throws<ArgumentException>(() => ProgramCatalog.BuildWeeklySchedule("bro-split", 3));
    }
}
