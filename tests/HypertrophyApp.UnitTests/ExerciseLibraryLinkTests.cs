using System.Text.Json;
using HypertrophyApp.Application.Programs;

namespace HypertrophyApp.UnitTests;

/// <summary>Guards the link between the built-in programs and the bundled exercise library.</summary>
public class ExerciseLibraryLinkTests
{
    private static readonly Lazy<Dictionary<string, int>> ImageCountById = new(() =>
    {
        var dir = new DirectoryInfo(AppContext.BaseDirectory);
        while (dir is not null && !File.Exists(Path.Combine(dir.FullName, "HypertrophyApp.slnx")))
            dir = dir.Parent;

        var path = Path.Combine(dir!.FullName, "src", "HypertrophyApp.Infrastructure", "Persistence", "Seed", "exercises.json");
        using var doc = JsonDocument.Parse(File.ReadAllText(path));
        return doc.RootElement.EnumerateArray().ToDictionary(
            e => e.GetProperty("id").GetString()!,
            e => e.GetProperty("images").GetArrayLength());
    });

    [Fact]
    public void Library_contains_the_expected_number_of_exercises() =>
        Assert.True(ImageCountById.Value.Count > 800);

    [Fact]
    public void Every_template_exercise_links_to_a_library_entry_with_a_demo()
    {
        foreach (var (name, id) in ProgramCatalog.LibraryIds)
        {
            Assert.True(ImageCountById.Value.TryGetValue(id, out var images), $"'{name}' → '{id}' is not in the library.");
            Assert.True(images >= 2, $"'{id}' has no start/end images.");
        }
    }

    [Fact]
    public void Every_exercise_in_every_built_in_program_has_a_library_link()
    {
        var unlinked = ProgramCatalog.GetSplits()
            .SelectMany(s => s.SupportedDays.SelectMany(d => ProgramCatalog.BuildWeeklySchedule(s.Id, d)))
            .SelectMany(d => d.Workout?.Exercises ?? [])
            .Where(e => e.ExerciseId is null)
            .Select(e => e.Name)
            .Distinct()
            .ToList();

        Assert.Empty(unlinked);
    }
}
