using HypertrophyApp.Application.Common;
using HypertrophyApp.Application.Programs;
using HypertrophyApp.Domain.Exercises;
using HypertrophyApp.Shared.Exercises;
using HypertrophyApp.Shared.Programs;
using Microsoft.EntityFrameworkCore;

namespace HypertrophyApp.Application.Exercises;

/// <summary>Where exercise demo images are served from (configurable so the media source can be swapped).</summary>
public sealed record ExerciseMediaOptions(string ImageBaseUrl);

/// <summary>Search and browse the exercise library, and attach demo images to workouts.</summary>
public sealed class ExerciseLibraryService(IApplicationDbContext db, ExerciseMediaOptions media)
{
    private static readonly string[] PopularIds = ProgramCatalog.LibraryIds.Values.Distinct().ToArray();

    public async Task<ExerciseSearchResultDto> SearchAsync(
        string? query, string? muscle, string? equipment, int skip, int take, CancellationToken ct)
    {
        var q = db.Exercises.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(query))
        {
            // Every word must appear in the name, so "db curl" style partial searches still work.
            foreach (var word in query.Trim().ToLowerInvariant().Split(' ', StringSplitOptions.RemoveEmptyEntries))
                q = q.Where(e => e.Name.ToLower().Contains(word));
        }
        if (!string.IsNullOrWhiteSpace(muscle))
            q = q.Where(e => e.PrimaryMuscles.Contains(muscle));
        if (!string.IsNullOrWhiteSpace(equipment))
            q = q.Where(e => e.Equipment == equipment);

        var total = await q.CountAsync(ct);
        var startsWith = query?.Trim().ToLowerInvariant() ?? string.Empty;
        var items = await q
            // Staple lifts used by the built-in programs first, so "squat" finds Barbell Squat before Squat Jerk.
            .OrderByDescending(e => PopularIds.Contains(e.Id))
            .ThenByDescending(e => startsWith != "" && e.Name.ToLower().StartsWith(startsWith))
            .ThenBy(e => e.Category != "strength") // strength exercises first
            .ThenBy(e => e.Name)
            .Skip(Math.Max(0, skip))
            .Take(Math.Clamp(take, 1, 100))
            .ToListAsync(ct);

        return new ExerciseSearchResultDto(items.Select(ToSummary).ToList(), total);
    }

    public async Task<ExerciseDetailDto?> GetAsync(string id, CancellationToken ct)
    {
        var e = await db.Exercises.AsNoTracking().FirstOrDefaultAsync(x => x.Id == id, ct);
        return e is null ? null : new ExerciseDetailDto(
            e.Id, e.Name, e.Category, e.Level, e.Equipment, e.Force, e.Mechanic,
            e.PrimaryMuscles, e.SecondaryMuscles, e.Instructions, Urls(e));
    }

    public async Task<ExerciseFiltersDto> GetFiltersAsync(CancellationToken ct)
    {
        var rows = await db.Exercises.AsNoTracking()
            .Select(e => new { e.PrimaryMuscles, e.Equipment })
            .ToListAsync(ct);

        return new ExerciseFiltersDto(
            rows.SelectMany(r => r.PrimaryMuscles).Distinct().Order().ToList(),
            rows.Select(r => r.Equipment).OfType<string>().Distinct().Order().ToList());
    }

    /// <summary>Fills in <see cref="ExerciseDto.ImageUrls"/> for every exercise linked to the library.</summary>
    public async Task<IReadOnlyList<ScheduleDayDto>> AttachMediaAsync(IReadOnlyList<ScheduleDayDto> week, CancellationToken ct)
    {
        var urls = await GetUrlsAsync(week.SelectMany(d => d.Workout?.Exercises ?? []), ct);
        return week.Select(d => d.Workout is null ? d : d with
        {
            Workout = d.Workout with { Exercises = d.Workout.Exercises.Select(e => WithMedia(e, urls)).ToList() }
        }).ToList();
    }

    public async Task<IReadOnlyList<CustomDayDto>> AttachMediaAsync(IReadOnlyList<CustomDayDto> days, CancellationToken ct)
    {
        var urls = await GetUrlsAsync(days.SelectMany(d => d.Exercises), ct);
        return days.Select(d => d with { Exercises = d.Exercises.Select(e => WithMedia(e, urls)).ToList() }).ToList();
    }

    private async Task<Dictionary<string, IReadOnlyList<string>>> GetUrlsAsync(IEnumerable<ExerciseDto> exercises, CancellationToken ct)
    {
        var ids = exercises.Select(e => e.ExerciseId).OfType<string>().Distinct().ToList();
        if (ids.Count == 0) return [];

        var found = await db.Exercises.AsNoTracking().Where(e => ids.Contains(e.Id)).ToListAsync(ct);
        return found.ToDictionary(e => e.Id, Urls);
    }

    private static ExerciseDto WithMedia(ExerciseDto e, Dictionary<string, IReadOnlyList<string>> urls) =>
        e.ExerciseId is { } id && urls.TryGetValue(id, out var u) ? e with { ImageUrls = u } : e;

    private ExerciseSummaryDto ToSummary(Exercise e) =>
        new(e.Id, e.Name, e.Level, e.Equipment, e.PrimaryMuscles, Urls(e));

    private IReadOnlyList<string> Urls(Exercise e) =>
        e.ImagePaths.Select(p => $"{media.ImageBaseUrl.TrimEnd('/')}/{p}").ToList();
}
