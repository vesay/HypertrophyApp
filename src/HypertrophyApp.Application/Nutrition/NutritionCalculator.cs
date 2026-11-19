using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Shared.Dashboard;

namespace HypertrophyApp.Application.Nutrition;

/// <summary>
/// Estimates daily energy needs (Mifflin-St Jeor BMR × activity factor) and macros.
/// </summary>
public static class NutritionCalculator
{
    public static NutritionPlanDto Calculate(UserProfile profile)
    {
        var sexConstant = profile.Sex == Sex.Female ? -161m : 5m;
        var bmr = 10m * profile.WeightKg + 6.25m * profile.HeightCm - 5m * profile.Age + sexConstant;
        var tdee = bmr * ActivityFactor(profile.DaysPerWeek);

        var target = profile.Goal switch
        {
            FitnessGoal.LoseFat => tdee * 0.80m,
            FitnessGoal.BuildMuscle => tdee * 1.10m,
            _ => tdee
        };
        var targetCalories = Math.Max(1200, RoundTo(target, 10));

        // For high BMI, base protein on the weight at BMI 25 to avoid unrealistic targets.
        var heightM = profile.HeightCm / 100m;
        var referenceWeight = profile.Bmi > 30 ? 25m * heightM * heightM : profile.WeightKg;
        var proteinPerKg = profile.Goal switch
        {
            FitnessGoal.LoseFat => 2.2m,
            FitnessGoal.BuildMuscle => 2.0m,
            _ => 1.8m
        };

        var protein = RoundTo(referenceWeight * proteinPerKg, 1);
        var fat = RoundTo(targetCalories * 0.25m / 9m, 1);
        var carbs = Math.Max(0, RoundTo((targetCalories - protein * 4 - fat * 9) / 4m, 1));
        var water = Math.Round(profile.WeightKg * 0.035m, 1);

        return new NutritionPlanDto(
            Bmr: RoundTo(bmr, 1),
            Tdee: RoundTo(tdee, 10),
            TargetCalories: targetCalories,
            ProteinGrams: protein,
            CarbGrams: carbs,
            FatGrams: fat,
            WaterLiters: water,
            Meals: BuildMeals(targetCalories, profile.Goal));
    }

    public static decimal ActivityFactor(int trainingDaysPerWeek) => trainingDaysPerWeek switch
    {
        <= 1 => 1.3m,
        2 => 1.375m,
        3 => 1.45m,
        4 => 1.55m,
        5 => 1.65m,
        _ => 1.725m
    };

    private static IReadOnlyList<MealDto> BuildMeals(int targetCalories, FitnessGoal goal)
    {
        var snackFoods = goal == FitnessGoal.BuildMuscle
            ? new[] { "Greek yogurt with honey", "A handful of almonds or walnuts", "Banana", "Peanut butter on whole-grain toast" }
            : new[] { "Greek yogurt or kefir", "An apple or a handful of berries", "A small handful of nuts" };

        return
        [
            new("Breakfast", Share(0.25m), ["Oatmeal cooked with milk", "2–3 eggs (boiled or scrambled)", "A banana or seasonal fruit"]),
            new("Lunch", Share(0.30m), ["Grilled chicken breast", "Rice or bulgur pilaf", "Large salad with olive oil"]),
            new("Snack", Share(0.15m), snackFoods),
            new("Dinner", Share(0.30m), ["Salmon, lean beef or lentils", "Baked potatoes or whole-grain pasta", "Steamed or roasted vegetables"]),
        ];

        int Share(decimal ratio) => RoundTo(targetCalories * ratio, 10);
    }

    private static int RoundTo(decimal value, int step) =>
        (int)(Math.Round(value / step, MidpointRounding.AwayFromZero) * step);
}
