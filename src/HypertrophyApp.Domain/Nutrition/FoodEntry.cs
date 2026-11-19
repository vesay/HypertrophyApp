namespace HypertrophyApp.Domain.Nutrition;

public enum MealType
{
    Breakfast = 0,
    Lunch = 1,
    Dinner = 2,
    Snack = 3
}

/// <summary>Something the user ate, logged against a calendar day.</summary>
public sealed class FoodEntry
{
    public const int MaxNameLength = 100;

    private FoodEntry() { } // EF Core

    public Guid Id { get; private set; }
    public Guid ProfileId { get; private set; }
    public DateOnly Date { get; private set; }
    public MealType Meal { get; private set; }
    public string Name { get; private set; } = string.Empty;
    public int Calories { get; private set; }
    public decimal? ProteinGrams { get; private set; }
    public decimal? CarbGrams { get; private set; }
    public decimal? FatGrams { get; private set; }
    public DateTime CreatedAtUtc { get; private set; }

    public static FoodEntry Create(
        Guid profileId, DateOnly date, MealType meal, string name, int calories,
        decimal? proteinGrams, decimal? carbGrams, decimal? fatGrams, DateTime nowUtc)
    {
        name = name?.Trim() ?? string.Empty;

        if (name.Length is 0 or > MaxNameLength)
            throw new ArgumentException($"Food name must be 1-{MaxNameLength} characters.", nameof(name));
        if (calories is < 0 or > 10_000)
            throw new ArgumentOutOfRangeException(nameof(calories), "Calories must be between 0 and 10,000.");
        if (!Enum.IsDefined(meal))
            throw new ArgumentOutOfRangeException(nameof(meal));
        foreach (var (value, label) in new[] { (proteinGrams, "Protein"), (carbGrams, "Carbs"), (fatGrams, "Fat") })
            if (value is < 0 or > 1000)
                throw new ArgumentOutOfRangeException(label, $"{label} must be between 0 and 1000 g.");

        return new FoodEntry
        {
            Id = Guid.NewGuid(),
            ProfileId = profileId,
            Date = date,
            Meal = meal,
            Name = name,
            Calories = calories,
            ProteinGrams = proteinGrams,
            CarbGrams = carbGrams,
            FatGrams = fatGrams,
            CreatedAtUtc = nowUtc
        };
    }
}
