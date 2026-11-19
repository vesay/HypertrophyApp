using System.ComponentModel.DataAnnotations;

namespace HypertrophyApp.Shared.Nutrition;

public enum Meal
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,
    Snack = 3
}

public sealed class AddFoodEntryRequest
{
    public DateOnly Date { get; set; } = DateOnly.FromDateTime(DateTime.Today);

    public Meal Meal { get; set; } = Meal.Breakfast;

    [Required(ErrorMessage = "What did you eat?")]
    [StringLength(100, ErrorMessage = "Name can be at most 100 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Enter the calories.")]
    [Range(0, 10_000, ErrorMessage = "Calories must be between 0 and 10,000.")]
    public int? Calories { get; set; }

    [Range(0, 1000, ErrorMessage = "0-1000 g")]
    public decimal? ProteinGrams { get; set; }

    [Range(0, 1000, ErrorMessage = "0-1000 g")]
    public decimal? CarbGrams { get; set; }

    [Range(0, 1000, ErrorMessage = "0-1000 g")]
    public decimal? FatGrams { get; set; }
}

public sealed record FoodEntryDto(
    Guid Id, DateOnly Date, Meal Meal, string Name, int Calories,
    decimal? ProteinGrams, decimal? CarbGrams, decimal? FatGrams);

public sealed record DailyIntakeDto(DateOnly Date, int Calories, decimal ProteinGrams, decimal CarbGrams, decimal FatGrams);
