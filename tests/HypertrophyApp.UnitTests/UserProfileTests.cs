using HypertrophyApp.Domain.Profiles;

namespace HypertrophyApp.UnitTests;

public class UserProfileTests
{
    [Fact]
    public void Create_trims_name_and_calculates_bmi()
    {
        var profile = UserProfile.Create("  Alex  ", Sex.Male, 28, 180, 81, 4, "upper-lower", FitnessGoal.Maintain, DateTime.UtcNow);

        Assert.Equal("Alex", profile.Name);
        Assert.Equal(25.0m, profile.Bmi);
    }

    [Theory]
    [InlineData("", 25, 175, 75, 3)]
    [InlineData("Ali", 10, 175, 75, 3)]
    [InlineData("Ali", 25, 100, 75, 3)]
    [InlineData("Ali", 25, 175, 20, 3)]
    [InlineData("Ali", 25, 175, 75, 0)]
    [InlineData("Ali", 25, 175, 75, 7)]
    public void Create_rejects_invalid_input(string name, int age, int height, int weight, int days) =>
        Assert.ThrowsAny<ArgumentException>(() =>
            UserProfile.Create(name, Sex.Female, age, height, weight, days, "full-body", FitnessGoal.Maintain, DateTime.UtcNow));
}
