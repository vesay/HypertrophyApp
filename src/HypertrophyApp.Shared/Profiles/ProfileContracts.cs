using System.ComponentModel.DataAnnotations;

namespace HypertrophyApp.Shared.Profiles;

public enum Goal
{
    LoseFat = 0,
    Maintain = 1,
    BuildMuscle = 2
}

public enum Sex
{
    Male = 0,
    Female = 1
}

/// <summary>
/// Used both to create and to update a profile. Shared by the API (automatic model validation)
/// and the Blazor forms (DataAnnotationsValidator), so validation rules are defined exactly once.
/// </summary>
public sealed class SaveProfileRequest
{
    [Required(ErrorMessage = "Please enter a name or nickname.")]
    [StringLength(50, ErrorMessage = "Name can be at most 50 characters.")]
    public string Name { get; set; } = string.Empty;

    [Required(ErrorMessage = "Please select your sex.")]
    public Sex? Sex { get; set; }

    [Range(13, 90, ErrorMessage = "Age must be between 13 and 90.")]
    public int Age { get; set; } = 25;

    [Range(120, 230, ErrorMessage = "Height must be between 120 and 230 cm.")]
    public decimal HeightCm { get; set; } = 175;

    [Range(30, 300, ErrorMessage = "Weight must be between 30 and 300 kg.")]
    public decimal WeightKg { get; set; } = 75;

    [Range(1, 6, ErrorMessage = "Choose between 1 and 6 training days.")]
    public int DaysPerWeek { get; set; } = 3;

    [Required(ErrorMessage = "Please choose a training split.")]
    public string SplitId { get; set; } = string.Empty;

    public Goal Goal { get; set; } = Goal.Maintain;

    public static SaveProfileRequest From(ProfileDto p) => new()
    {
        Name = p.Name, Sex = p.Sex, Age = p.Age, HeightCm = p.HeightCm, WeightKg = p.WeightKg,
        DaysPerWeek = p.DaysPerWeek, SplitId = p.SplitId, Goal = p.Goal
    };
}

public sealed record ProfileDto(
    Guid Id, string Name, Sex Sex, int Age, decimal HeightCm, decimal WeightKg,
    decimal Bmi, string BmiCategory, int DaysPerWeek, string SplitId, Goal Goal,
    bool HasCustomProgram, DateTime CreatedAtUtc);
