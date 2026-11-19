using HypertrophyApp.Application.Common;
using HypertrophyApp.Application.Exercises;
using HypertrophyApp.Application.Nutrition;
using HypertrophyApp.Application.Programs;
using HypertrophyApp.Domain.Profiles;
using HypertrophyApp.Shared.Dashboard;
using HypertrophyApp.Shared.Profiles;
using HypertrophyApp.Shared.Programs;
using Microsoft.EntityFrameworkCore;
using DomainSex = HypertrophyApp.Domain.Profiles.Sex;
using SharedSex = HypertrophyApp.Shared.Profiles.Sex;

namespace HypertrophyApp.Application.Profiles;

public sealed class ProfileService(IApplicationDbContext db, ExerciseLibraryService library, TimeProvider clock)
{
    public async Task<Result<ProfileDto>> CreateAsync(SaveProfileRequest request, CancellationToken ct)
    {
        if (ValidateTemplateSplit(request) is { } error)
            return Result<ProfileDto>.Failure(ErrorType.Validation, error);

        UserProfile profile;
        try
        {
            profile = UserProfile.Create(
                request.Name, (DomainSex)request.Sex!.Value, request.Age, request.HeightCm, request.WeightKg,
                request.DaysPerWeek, request.SplitId, (FitnessGoal)request.Goal, Now);
        }
        catch (ArgumentException ex)
        {
            return Result<ProfileDto>.Failure(ErrorType.Validation, ex.Message);
        }

        db.Profiles.Add(profile);
        await db.SaveChangesAsync(ct);
        return ToDto(profile);
    }

    public async Task<Result<ProfileDto>> UpdateAsync(Guid id, SaveProfileRequest request, CancellationToken ct)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null) return NotFound<ProfileDto>(id);

        if (request.SplitId != UserProfile.CustomSplitId && ValidateTemplateSplit(request) is { } error)
            return Result<ProfileDto>.Failure(ErrorType.Validation, error);

        try
        {
            profile.Update(
                request.Name, (DomainSex)request.Sex!.Value, request.Age, request.HeightCm, request.WeightKg,
                request.DaysPerWeek, request.SplitId, (FitnessGoal)request.Goal, Now);
        }
        catch (ArgumentException ex)
        {
            return Result<ProfileDto>.Failure(ErrorType.Validation, ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return ToDto(profile);
    }

    /// <summary>Deletes the profile and, via cascade, every workout and food log that belongs to it.</summary>
    public async Task<bool> DeleteAsync(Guid id, CancellationToken ct) =>
        await db.Profiles.Where(p => p.Id == id).ExecuteDeleteAsync(ct) > 0;

    public async Task<Result<ProfileDto>> GetAsync(Guid id, CancellationToken ct)
    {
        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return profile is null ? NotFound<ProfileDto>(id) : ToDto(profile);
    }

    public async Task<Result<DashboardDto>> GetDashboardAsync(Guid id, CancellationToken ct)
    {
        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null) return NotFound<DashboardDto>(id);

        return new DashboardDto(
            ToDto(profile),
            ScheduleBuilder.Split(profile),
            await library.AttachMediaAsync(ScheduleBuilder.Build(profile), ct),
            NutritionCalculator.Calculate(profile));
    }

    public async Task<Result<IReadOnlyList<CustomDayDto>>> GetCustomProgramAsync(Guid id, CancellationToken ct)
    {
        var profile = await db.Profiles.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id, ct);
        return profile is null
            ? NotFound<IReadOnlyList<CustomDayDto>>(id)
            : Result<IReadOnlyList<CustomDayDto>>.Success(await library.AttachMediaAsync(ScheduleBuilder.ToDtos(profile.CustomProgram), ct));
    }

    public async Task<Result<ProfileDto>> SaveCustomProgramAsync(Guid id, SaveCustomProgramRequest request, CancellationToken ct)
    {
        var profile = await db.Profiles.FirstOrDefaultAsync(p => p.Id == id, ct);
        if (profile is null) return NotFound<ProfileDto>(id);

        try
        {
            var days = (request.Days ?? []).Select(d =>
            {
                if (!Enum.TryParse<DayOfWeek>(d.DayOfWeek, ignoreCase: true, out var day))
                    throw new ArgumentException($"'{d.DayOfWeek}' is not a day of the week.");

                return CustomWorkoutDay.Create(day, d.Title,
                    (d.Exercises ?? []).Select(e => CustomExercise.Create(e.Name, e.Sets, e.Reps, e.RestSeconds, e.ExerciseId)));
            });

            profile.SetCustomProgram(days, Now);
        }
        catch (ArgumentException ex)
        {
            return Result<ProfileDto>.Failure(ErrorType.Validation, ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return ToDto(profile);
    }

    private static string? ValidateTemplateSplit(SaveProfileRequest request) =>
        ProgramCatalog.Supports(request.SplitId, request.DaysPerWeek)
            ? null
            : $"The '{request.SplitId}' split is not available for {request.DaysPerWeek} training days.";

    private DateTime Now => clock.GetUtcNow().UtcDateTime;

    private static Result<T> NotFound<T>(Guid id) =>
        Result<T>.Failure(ErrorType.NotFound, $"Profile '{id}' was not found.");

    private static ProfileDto ToDto(UserProfile p) => new(
        p.Id, p.Name, (SharedSex)p.Sex, p.Age, p.HeightCm, p.WeightKg, p.Bmi, BmiCategory(p.Bmi),
        p.DaysPerWeek, p.SplitId, (Goal)p.Goal, p.CustomProgram.Count > 0, p.CreatedAtUtc);

    public static string BmiCategory(decimal bmi) => bmi switch
    {
        < 18.5m => "Underweight",
        < 25m => "Normal",
        < 30m => "Overweight",
        _ => "Obese"
    };
}
