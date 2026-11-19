using HypertrophyApp.Application.Common;
using HypertrophyApp.Domain.Nutrition;
using HypertrophyApp.Shared.Nutrition;
using Microsoft.EntityFrameworkCore;

namespace HypertrophyApp.Application.Nutrition;

/// <summary>A simple calorie diary: what the user ate, per day.</summary>
public sealed class FoodLogService(IApplicationDbContext db, TimeProvider clock)
{
    public async Task<IReadOnlyList<FoodEntryDto>> GetDayAsync(Guid profileId, DateOnly date, CancellationToken ct)
    {
        var entries = await db.FoodEntries.AsNoTracking()
            .Where(f => f.ProfileId == profileId && f.Date == date)
            .ToListAsync(ct);

        // Meal is stored as text, so cast and order in memory (SQL would sort it alphabetically).
        return entries
            .OrderBy(f => f.Meal).ThenBy(f => f.CreatedAtUtc)
            .Select(ToDto)
            .ToList();
    }

    public async Task<Result<FoodEntryDto>> AddAsync(Guid profileId, AddFoodEntryRequest request, CancellationToken ct)
    {
        if (!await db.Profiles.AnyAsync(p => p.Id == profileId, ct))
            return Result<FoodEntryDto>.Failure(ErrorType.NotFound, "Profile not found.");
        if (DateRules.Validate(request.Date, clock) is { } dateError)
            return Result<FoodEntryDto>.Failure(ErrorType.Validation, dateError);

        FoodEntry entry;
        try
        {
            entry = FoodEntry.Create(profileId, request.Date, (MealType)request.Meal, request.Name, request.Calories ?? 0,
                request.ProteinGrams, request.CarbGrams, request.FatGrams, clock.GetUtcNow().UtcDateTime);
        }
        catch (ArgumentException ex)
        {
            return Result<FoodEntryDto>.Failure(ErrorType.Validation, ex.Message);
        }

        db.FoodEntries.Add(entry);
        await db.SaveChangesAsync(ct);
        return ToDto(entry);
    }

    private static FoodEntryDto ToDto(FoodEntry f) =>
        new(f.Id, f.Date, (Meal)f.Meal, f.Name, f.Calories, f.ProteinGrams, f.CarbGrams, f.FatGrams);

    public async Task<bool> DeleteAsync(Guid profileId, Guid entryId, CancellationToken ct) =>
        await db.FoodEntries.Where(f => f.ProfileId == profileId && f.Id == entryId).ExecuteDeleteAsync(ct) > 0;

    /// <summary>Daily totals for a date range (days without entries are omitted).</summary>
    public async Task<IReadOnlyList<DailyIntakeDto>> GetDailyTotalsAsync(Guid profileId, DateOnly from, DateOnly to, CancellationToken ct) =>
        await db.FoodEntries.AsNoTracking()
            .Where(f => f.ProfileId == profileId && f.Date >= from && f.Date <= to)
            .GroupBy(f => f.Date)
            .OrderBy(g => g.Key)
            .Select(g => new DailyIntakeDto(
                g.Key,
                g.Sum(f => f.Calories),
                g.Sum(f => f.ProteinGrams ?? 0),
                g.Sum(f => f.CarbGrams ?? 0),
                g.Sum(f => f.FatGrams ?? 0)))
            .ToListAsync(ct);
}
