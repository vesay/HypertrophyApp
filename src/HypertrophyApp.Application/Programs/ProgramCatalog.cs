using HypertrophyApp.Shared.Programs;

namespace HypertrophyApp.Application.Programs;

/// <summary>
/// Built-in catalogue of popular training splits. Each split is a rotation of workout days
/// that is spread over the number of training days the user chose.
/// </summary>
/// <remarks>
/// Recommendations follow the resistance-training frequency research: training each muscle group
/// about twice a week tends to beat once a week (Schoenfeld et al., 2016), while going beyond that adds
/// little when weekly volume is equal. One full-body session a week still works if it reaches roughly
/// 4+ hard sets per muscle group with compound lifts. So: 1–3 days → full body, 4 → upper/lower,
/// 5 → upper/lower + PPL hybrid, 6 → PPL twice. Once-a-week body-part splits are offered as alternatives only.
/// </remarks>
public static class ProgramCatalog
{
    /// <param name="HitsPerRotation">How many times each major muscle group is trained in one pass of <paramref name="Rotation"/>.</param>
    /// <param name="RecommendedDays">Day counts for which this split is the evidence-based default.</param>
    private sealed record SplitDefinition(
        string Id, string Name, string Description, string Level,
        int[] SupportedDays, int[] RecommendedDays, int HitsPerRotation, WorkoutDayDto[] Rotation);

    // Training days are placed so that rest days are spread through the week.
    private static readonly Dictionary<int, DayOfWeek[]> TrainingDayLayout = new()
    {
        [1] = [DayOfWeek.Saturday],
        [2] = [DayOfWeek.Monday, DayOfWeek.Thursday],
        [3] = [DayOfWeek.Monday, DayOfWeek.Wednesday, DayOfWeek.Friday],
        [4] = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Thursday, DayOfWeek.Friday],
        [5] = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Friday, DayOfWeek.Saturday],
        [6] = [DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday, DayOfWeek.Friday, DayOfWeek.Saturday],
    };

    private static readonly DayOfWeek[] Week =
    [
        DayOfWeek.Monday, DayOfWeek.Tuesday, DayOfWeek.Wednesday, DayOfWeek.Thursday,
        DayOfWeek.Friday, DayOfWeek.Saturday, DayOfWeek.Sunday
    ];

    /// <summary>Links the built-in programs to the exercise library so every exercise shows its form demo.</summary>
    public static readonly IReadOnlyDictionary<string, string> LibraryIds = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
    {
        ["Barbell Back Squat"] = "Barbell_Squat",
        ["Barbell Bench Press"] = "Barbell_Bench_Press_-_Medium_Grip",
        ["Barbell Row"] = "Bent_Over_Barbell_Row",
        ["Overhead Press"] = "Standing_Military_Press",
        ["Plank"] = "Plank",
        ["Romanian Deadlift"] = "Romanian_Deadlift",
        ["Incline Dumbbell Press"] = "Incline_Dumbbell_Press",
        ["Lat Pulldown"] = "Wide-Grip_Lat_Pulldown",
        ["Walking Lunge"] = "Bodyweight_Walking_Lunge",
        ["Hanging Knee Raise"] = "Hanging_Leg_Raise",
        ["Conventional Deadlift"] = "Barbell_Deadlift",
        ["Pull-Up"] = "Pullups",
        ["Leg Press"] = "Leg_Press",
        ["Dumbbell Curl"] = "Dumbbell_Bicep_Curl",
        ["Lateral Raise"] = "Side_Lateral_Raise",
        ["Triceps Pushdown"] = "Triceps_Pushdown",
        ["Lying Leg Curl"] = "Lying_Leg_Curls",
        ["Standing Calf Raise"] = "Standing_Calf_Raises",
        ["Overhead Triceps Extension"] = "Cable_Rope_Overhead_Triceps_Extension",
        ["Seated Cable Row"] = "Seated_Cable_Rows",
        ["Face Pull"] = "Face_Pull",
        ["Barbell Curl"] = "Barbell_Curl",
        ["Hammer Curl"] = "Hammer_Curls",
        ["Hanging Leg Raise"] = "Hanging_Leg_Raise",
        ["Weighted Dip"] = "Dips_-_Chest_Version",
        ["Cable Fly"] = "Flat_Bench_Cable_Flyes",
        ["Push-Up"] = "Pushups",
        ["Straight-Arm Pulldown"] = "Straight-Arm_Pulldown",
        ["Seated Dumbbell Press"] = "Seated_Dumbbell_Press",
        ["Reverse Pec Deck"] = "Reverse_Flyes",
        ["Dumbbell Shrug"] = "Dumbbell_Shrug",
        ["Close-Grip Bench Press"] = "Close-Grip_Barbell_Bench_Press",
        ["Skull Crusher"] = "EZ-Bar_Skullcrusher",
        ["Incline Dumbbell Curl"] = "Incline_Dumbbell_Curl",
    };

    private static ExerciseDto Ex(string name, int sets, string reps, int rest = 90) =>
        new(name, sets, reps, rest, LibraryIds.GetValueOrDefault(name));

    #region Workout templates
    private static readonly WorkoutDayDto FullBodyA = new("Full Body A", "Squat & horizontal press", [
        Ex("Barbell Back Squat", 3, "6-8", 150), Ex("Barbell Bench Press", 3, "6-8", 150),
        Ex("Barbell Row", 3, "8-10", 120), Ex("Overhead Press", 2, "8-10"), Ex("Plank", 3, "45 sec", 60)]);
    private static readonly WorkoutDayDto FullBodyB = new("Full Body B", "Hinge & incline press", [
        Ex("Romanian Deadlift", 3, "8-10", 120), Ex("Incline Dumbbell Press", 3, "8-10"),
        Ex("Lat Pulldown", 3, "10-12"), Ex("Walking Lunge", 2, "10 / leg"), Ex("Hanging Knee Raise", 3, "12", 60)]);
    private static readonly WorkoutDayDto FullBodyC = new("Full Body C", "Deadlift & vertical press", [
        Ex("Conventional Deadlift", 3, "5", 180), Ex("Overhead Press", 3, "6-8", 120),
        Ex("Pull-Up", 3, "AMRAP", 120), Ex("Leg Press", 3, "10-12"), Ex("Dumbbell Curl", 2, "12", 60)]);

    private static readonly WorkoutDayDto Upper = new("Upper Body", "Chest, back, shoulders, arms", [
        Ex("Barbell Bench Press", 4, "6-8", 150), Ex("Barbell Row", 4, "6-8", 120), Ex("Overhead Press", 3, "8-10"),
        Ex("Lat Pulldown", 3, "10-12"), Ex("Lateral Raise", 3, "12-15", 60),
        Ex("Triceps Pushdown", 2, "12", 60), Ex("Dumbbell Curl", 2, "12", 60)]);
    private static readonly WorkoutDayDto Lower = new("Lower Body", "Quads, hamstrings, glutes, calves", [
        Ex("Barbell Back Squat", 4, "6-8", 150), Ex("Romanian Deadlift", 3, "8-10", 120), Ex("Leg Press", 3, "10-12"),
        Ex("Lying Leg Curl", 3, "12", 60), Ex("Standing Calf Raise", 4, "12-15", 60), Ex("Plank", 3, "45 sec", 60)]);

    private static readonly WorkoutDayDto Push = new("Push", "Chest, shoulders, triceps", [
        Ex("Barbell Bench Press", 4, "6-8", 150), Ex("Overhead Press", 3, "8-10", 120), Ex("Incline Dumbbell Press", 3, "10-12"),
        Ex("Lateral Raise", 3, "12-15", 60), Ex("Triceps Pushdown", 3, "12", 60), Ex("Overhead Triceps Extension", 2, "12", 60)]);
    private static readonly WorkoutDayDto Pull = new("Pull", "Back, rear delts, biceps", [
        Ex("Pull-Up", 4, "6-10", 120), Ex("Barbell Row", 3, "8-10", 120), Ex("Seated Cable Row", 3, "10-12"),
        Ex("Face Pull", 3, "15", 60), Ex("Barbell Curl", 3, "10", 60), Ex("Hammer Curl", 2, "12", 60)]);
    private static readonly WorkoutDayDto Legs = new("Legs", "Quads, hamstrings, glutes, calves", [
        Ex("Barbell Back Squat", 4, "6-8", 150), Ex("Romanian Deadlift", 3, "8-10", 120), Ex("Leg Press", 3, "10-12"),
        Ex("Lying Leg Curl", 3, "12", 60), Ex("Standing Calf Raise", 4, "15", 60), Ex("Hanging Leg Raise", 3, "12", 60)]);

    private static readonly WorkoutDayDto BroChest = new("Chest", "Chest focus", [
        Ex("Barbell Bench Press", 4, "6-8", 150), Ex("Incline Dumbbell Press", 3, "8-10"), Ex("Weighted Dip", 3, "8-10"),
        Ex("Cable Fly", 3, "12-15", 60), Ex("Push-Up", 2, "AMRAP", 60)]);
    private static readonly WorkoutDayDto BroBack = new("Back", "Back focus", [
        Ex("Conventional Deadlift", 3, "5", 180), Ex("Pull-Up", 4, "6-10", 120), Ex("Barbell Row", 3, "8-10"),
        Ex("Seated Cable Row", 3, "10-12"), Ex("Straight-Arm Pulldown", 2, "12-15", 60)]);
    private static readonly WorkoutDayDto BroShoulders = new("Shoulders", "Delts & traps", [
        Ex("Overhead Press", 4, "6-8", 150), Ex("Seated Dumbbell Press", 3, "8-10"), Ex("Lateral Raise", 4, "12-15", 60),
        Ex("Reverse Pec Deck", 3, "15", 60), Ex("Dumbbell Shrug", 3, "12", 60)]);
    private static readonly WorkoutDayDto BroArms = new("Arms", "Biceps & triceps", [
        Ex("Close-Grip Bench Press", 3, "8-10", 120), Ex("Barbell Curl", 3, "8-10"), Ex("Skull Crusher", 3, "10-12"),
        Ex("Incline Dumbbell Curl", 3, "10-12", 60), Ex("Triceps Pushdown", 3, "12-15", 60), Ex("Hammer Curl", 2, "12", 60)]);

    private static readonly WorkoutDayDto ArnoldChestBack = new("Chest & Back", "Antagonist supersets", [
        Ex("Barbell Bench Press", 4, "6-8", 120), Ex("Pull-Up", 4, "6-10", 120), Ex("Incline Dumbbell Press", 3, "8-10"),
        Ex("Barbell Row", 3, "8-10"), Ex("Cable Fly", 3, "12-15", 60), Ex("Straight-Arm Pulldown", 3, "12-15", 60)]);
    private static readonly WorkoutDayDto ArnoldShouldersArms = new("Shoulders & Arms", "Delts, biceps, triceps", [
        Ex("Overhead Press", 4, "6-8", 120), Ex("Lateral Raise", 4, "12-15", 60), Ex("Barbell Curl", 3, "8-10"),
        Ex("Close-Grip Bench Press", 3, "8-10"), Ex("Hammer Curl", 3, "12", 60), Ex("Overhead Triceps Extension", 3, "12", 60)]);

    // A single weekly session: compound lifts only, ~4-6 hard sets per major muscle group.
    private static readonly WorkoutDayDto FullBodyMinimal = new("Full Body", "All major muscles in one session", [
        Ex("Barbell Back Squat", 3, "6-10", 150), Ex("Romanian Deadlift", 3, "8-10", 120),
        Ex("Barbell Bench Press", 4, "6-10", 150), Ex("Barbell Row", 3, "8-12", 120),
        Ex("Lat Pulldown", 2, "10-12"), Ex("Overhead Press", 2, "8-12"), Ex("Plank", 2, "45 sec", 60)]);
    #endregion

    private static readonly SplitDefinition[] Splits =
    [
        new("full-body-minimal", "Full Body (Minimal)",
            "One efficient session with compound lifts and 4+ hard sets per muscle. Enough to build strength and keep muscle on a busy schedule.",
            "Beginner", SupportedDays: [1], RecommendedDays: [1], HitsPerRotation: 1, [FullBodyMinimal]),
        new("full-body", "Full Body",
            "Every major muscle each session with rotating A/B/C workouts. The best use of 2–3 days, and great for beginners.",
            "Beginner", SupportedDays: [2, 3, 4], RecommendedDays: [2, 3], HitsPerRotation: 3, [FullBodyA, FullBodyB, FullBodyC]),
        new("upper-lower", "Upper / Lower",
            "Alternate upper- and lower-body days. With 4 days it hits every muscle twice a week with good recovery.",
            "Beginner – Intermediate", SupportedDays: [2, 4], RecommendedDays: [4], HitsPerRotation: 1, [Upper, Lower]),
        new("push-pull-legs", "Push / Pull / Legs",
            "Pushing muscles, pulling muscles and legs. Run twice over 6 days it trains everything twice a week.",
            "Intermediate", SupportedDays: [3, 6], RecommendedDays: [6], HitsPerRotation: 1, [Push, Pull, Legs]),
        new("upper-lower-ppl", "Upper / Lower + PPL",
            "A 5-day hybrid: an upper/lower block followed by push, pull and legs – every muscle twice a week.",
            "Intermediate", SupportedDays: [5], RecommendedDays: [5], HitsPerRotation: 2, [Upper, Lower, Push, Pull, Legs]),
        new("bro-split", "Bro Split",
            "One muscle group per day with high volume. Popular, but each muscle is trained only once a week.",
            "Intermediate – Advanced", SupportedDays: [5], RecommendedDays: [], HitsPerRotation: 1, [BroChest, BroBack, BroShoulders, Legs, BroArms]),
        new("arnold", "Arnold Split",
            "Chest & back, shoulders & arms, legs – each twice a week. Demanding, high volume.",
            "Advanced", SupportedDays: [6], RecommendedDays: [], HitsPerRotation: 1, [ArnoldChestBack, ArnoldShouldersArms, Legs]),
    ];

    /// <summary>Lists splits, optionally only those supporting the given day count (recommended split first).</summary>
    public static IReadOnlyList<SplitSummaryDto> GetSplits(int? daysPerWeek = null) =>
        Splits.Where(s => daysPerWeek is null || s.SupportedDays.Contains(daysPerWeek.Value))
              .Select(s => ToSummary(s, daysPerWeek))
              .OrderByDescending(s => s.IsRecommended)
              .ToList();

    public static SplitSummaryDto? FindSplit(string splitId, int? daysPerWeek = null) =>
        Splits.FirstOrDefault(s => s.Id == splitId) is { } s ? ToSummary(s, daysPerWeek) : null;

    public static string? RecommendedSplitId(int daysPerWeek) =>
        Splits.FirstOrDefault(s => s.RecommendedDays.Contains(daysPerWeek))?.Id;

    public static bool Supports(string splitId, int daysPerWeek) =>
        Splits.Any(s => s.Id == splitId && s.SupportedDays.Contains(daysPerWeek));

    /// <summary>Builds a Monday–Sunday schedule by cycling the split's workouts over the training days.</summary>
    public static IReadOnlyList<ScheduleDayDto> BuildWeeklySchedule(string splitId, int daysPerWeek)
    {
        var split = Splits.FirstOrDefault(s => s.Id == splitId)
            ?? throw new ArgumentException($"Unknown split '{splitId}'.", nameof(splitId));
        if (!split.SupportedDays.Contains(daysPerWeek))
            throw new ArgumentException($"Split '{splitId}' does not support {daysPerWeek} days.", nameof(daysPerWeek));

        var trainingDays = TrainingDayLayout[daysPerWeek];
        var schedule = new List<ScheduleDayDto>(7);
        var workoutIndex = 0;

        foreach (var day in Week)
        {
            WorkoutDayDto? workout = null;
            if (trainingDays.Contains(day))
                workout = split.Rotation[workoutIndex++ % split.Rotation.Length];

            schedule.Add(new ScheduleDayDto(day.ToString(), workout));
        }

        return schedule;
    }

    /// <summary>How many times per week each major muscle group is trained with this split and day count.</summary>
    public static decimal TimesPerWeekPerMuscle(string splitId, int daysPerWeek)
    {
        var split = Splits.First(s => s.Id == splitId);
        return Math.Round((decimal)daysPerWeek * split.HitsPerRotation / split.Rotation.Length, 1);
    }

    private static SplitSummaryDto ToSummary(SplitDefinition s, int? daysPerWeek)
    {
        var frequencies = (daysPerWeek is { } d ? [d] : s.SupportedDays)
            .Select(days => TimesPerWeekPerMuscle(s.Id, days))
            .Distinct()
            .Order()
            .ToList();
        var frequency = string.Join("–", new[] { frequencies[0], frequencies[^1] }.Distinct().Select(f => f.ToString("0.#", System.Globalization.CultureInfo.InvariantCulture)));

        return new SplitSummaryDto(
            s.Id, s.Name, s.Description, s.SupportedDays, s.Level,
            TimesPerWeekPerMuscle: $"{frequency}× / week per muscle",
            IsRecommended: daysPerWeek is { } days && s.RecommendedDays.Contains(days));
    }
}
