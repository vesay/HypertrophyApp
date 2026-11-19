using HypertrophyApp.Shared.Profiles;
using HypertrophyApp.Shared.Programs;

namespace HypertrophyApp.Shared.Dashboard;

public sealed record MealDto(string Name, int Calories, IReadOnlyList<string> Foods);

public sealed record NutritionPlanDto(
    int Bmr, int Tdee, int TargetCalories,
    int ProteinGrams, int CarbGrams, int FatGrams,
    decimal WaterLiters, IReadOnlyList<MealDto> Meals);

public sealed record DashboardDto(
    ProfileDto Profile,
    SplitSummaryDto Split,
    IReadOnlyList<ScheduleDayDto> WeeklySchedule,
    NutritionPlanDto Nutrition);
