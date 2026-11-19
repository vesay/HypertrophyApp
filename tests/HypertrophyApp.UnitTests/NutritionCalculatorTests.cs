using HypertrophyApp.Application.Nutrition;
using HypertrophyApp.Domain.Profiles;

namespace HypertrophyApp.UnitTests;

public class NutritionCalculatorTests
{
    private static UserProfile Profile(decimal weight = 80, decimal height = 180, int age = 30,
        int days = 4, FitnessGoal goal = FitnessGoal.Maintain, Sex sex = Sex.Male) =>
        UserProfile.Create("Test", sex, age, height, weight, days, "upper-lower", goal, DateTime.UtcNow);

    [Theory]
    [InlineData(Sex.Male, 1780)]   // 10*80 + 6.25*180 - 5*30 + 5
    [InlineData(Sex.Female, 1614)] // 10*80 + 6.25*180 - 5*30 - 161
    public void Bmr_uses_mifflin_st_jeor_with_sex_constant(Sex sex, int expectedBmr) =>
        Assert.Equal(expectedBmr, NutritionCalculator.Calculate(Profile(sex: sex)).Bmr);

    [Theory]
    [InlineData(FitnessGoal.LoseFat, 0.80)]
    [InlineData(FitnessGoal.Maintain, 1.00)]
    [InlineData(FitnessGoal.BuildMuscle, 1.10)]
    public void Target_calories_follow_goal(FitnessGoal goal, double multiplier)
    {
        var plan = NutritionCalculator.Calculate(Profile(goal: goal));
        var expected = 1780m * 1.55m * (decimal)multiplier;
        Assert.InRange(plan.TargetCalories, expected - 10, expected + 10);
    }

    [Fact]
    public void Macros_add_up_to_target_calories()
    {
        var plan = NutritionCalculator.Calculate(Profile(goal: FitnessGoal.BuildMuscle));
        var macroCalories = plan.ProteinGrams * 4 + plan.CarbGrams * 4 + plan.FatGrams * 9;
        Assert.InRange(macroCalories, plan.TargetCalories - 15, plan.TargetCalories + 15);
    }

    [Fact]
    public void Protein_is_capped_for_high_bmi()
    {
        // 150 kg at 180 cm => BMI 46; protein should be based on BMI-25 weight (81 kg), not 150 kg.
        var plan = NutritionCalculator.Calculate(Profile(weight: 150));
        Assert.InRange(plan.ProteinGrams, 140, 150);
    }

    [Fact]
    public void Calories_never_drop_below_safe_minimum()
    {
        var plan = NutritionCalculator.Calculate(
            Profile(weight: 40, height: 140, age: 80, days: 2, goal: FitnessGoal.LoseFat, sex: Sex.Female));
        Assert.True(plan.TargetCalories >= 1200);
    }

    [Fact]
    public void More_training_days_mean_higher_energy_needs()
    {
        var factors = Enumerable.Range(1, 6).Select(NutritionCalculator.ActivityFactor).ToList();
        Assert.Equal(factors.Order(), factors);
        Assert.Equal(6, factors.Distinct().Count());
    }

    [Fact]
    public void Meal_calories_cover_the_daily_target()
    {
        var plan = NutritionCalculator.Calculate(Profile());
        Assert.Equal(4, plan.Meals.Count);
        Assert.InRange(plan.Meals.Sum(m => m.Calories), plan.TargetCalories - 20, plan.TargetCalories + 20);
    }
}
