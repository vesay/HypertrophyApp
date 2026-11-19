using HypertrophyApp.Application.Common;
using HypertrophyApp.Domain.Workouts;
using HypertrophyApp.Shared.Workouts;
using Microsoft.EntityFrameworkCore;

namespace HypertrophyApp.Application.Workouts;

/// <summary>Records performed sets and turns them into progress over time.</summary>
public sealed class WorkoutLogService(IApplicationDbContext db, TimeProvider clock)
{
    public async Task<WorkoutSessionDto?> GetSessionAsync(Guid profileId, DateOnly date, CancellationToken ct)
    {
        var session = await db.WorkoutSessions.AsNoTracking()
            .FirstOrDefaultAsync(s => s.ProfileId == profileId && s.Date == date, ct);
        return session is null ? null : ToDto(session);
    }

    /// <summary>Creates or replaces the session for a day (the client always sends the full day).</summary>
    public async Task<Result<WorkoutSessionDto>> SaveSessionAsync(
        Guid profileId, DateOnly date, SaveWorkoutSessionRequest request, CancellationToken ct)
    {
        if (!await db.Profiles.AnyAsync(p => p.Id == profileId, ct))
            return Result<WorkoutSessionDto>.Failure(ErrorType.NotFound, "Profile not found.");
        if (DateRules.Validate(date, clock) is { } dateError)
            return Result<WorkoutSessionDto>.Failure(ErrorType.Validation, dateError);

        var now = clock.GetUtcNow().UtcDateTime;
        var session = await db.WorkoutSessions.FirstOrDefaultAsync(s => s.ProfileId == profileId && s.Date == date, ct);

        try
        {
            var sets = (request.Sets ?? []).Select(s => SetLog.Create(s.ExerciseName, s.SetNumber, s.Reps, s.WeightKg)).ToList();

            if (session is null)
                db.WorkoutSessions.Add(session = WorkoutSession.Create(profileId, date, request.Title, sets, now));
            else
                session.Replace(request.Title, sets, now);
        }
        catch (ArgumentException ex)
        {
            return Result<WorkoutSessionDto>.Failure(ErrorType.Validation, ex.Message);
        }

        await db.SaveChangesAsync(ct);
        return ToDto(session);
    }

    public async Task<bool> DeleteSessionAsync(Guid profileId, DateOnly date, CancellationToken ct) =>
        await db.WorkoutSessions.Where(s => s.ProfileId == profileId && s.Date == date).ExecuteDeleteAsync(ct) > 0;

    public async Task<IReadOnlyList<WorkoutSessionSummaryDto>> GetHistoryAsync(Guid profileId, int take, CancellationToken ct)
    {
        var sessions = await db.WorkoutSessions.AsNoTracking()
            .Where(s => s.ProfileId == profileId)
            .OrderByDescending(s => s.Date)
            .Take(Math.Clamp(take, 1, 365))
            .ToListAsync(ct);

        return sessions.Select(s => new WorkoutSessionSummaryDto(
                s.Date, s.Title,
                s.Sets.Select(x => x.ExerciseName).Distinct(StringComparer.OrdinalIgnoreCase).Count(),
                s.Sets.Count, s.TotalReps, s.TotalVolumeKg))
            .ToList();
    }

    /// <summary>Exercise names the user has logged, most recently trained first.</summary>
    public async Task<IReadOnlyList<string>> GetLoggedExercisesAsync(Guid profileId, CancellationToken ct)
    {
        var sessions = await LoadAllAsync(profileId, ct);
        return sessions
            .OrderByDescending(s => s.Date)
            .SelectMany(s => s.Sets.Select(x => x.ExerciseName))
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToList();
    }

    public async Task<ExerciseProgressDto> GetProgressAsync(Guid profileId, string exerciseName, CancellationToken ct)
    {
        var sessions = await LoadAllAsync(profileId, ct);

        var points = sessions
            .OrderBy(s => s.Date)
            .Select(s => (s.Date, Sets: s.Sets.Where(x => string.Equals(x.ExerciseName, exerciseName, StringComparison.OrdinalIgnoreCase)).ToList()))
            .Where(x => x.Sets.Count > 0)
            .Select(x => new ExerciseProgressPointDto(
                x.Date,
                BestWeightKg: x.Sets.Max(s => s.WeightKg),
                BestReps: x.Sets.Max(s => s.Reps),
                TotalReps: x.Sets.Sum(s => s.Reps),
                VolumeKg: x.Sets.Sum(s => s.Reps * (s.WeightKg ?? 0)),
                Estimated1RmKg: x.Sets.Max(s => s.Estimated1RmKg)))
            .ToList();

        // Return the name as the user logged it, not as typed in the query.
        var canonicalName = sessions.SelectMany(s => s.Sets).Select(s => s.ExerciseName)
            .FirstOrDefault(n => string.Equals(n, exerciseName, StringComparison.OrdinalIgnoreCase)) ?? exerciseName;

        return new ExerciseProgressDto(canonicalName, points);
    }

    // A personal log is small (a few hundred sessions at most), so aggregating in memory keeps the code simple.
    private Task<List<WorkoutSession>> LoadAllAsync(Guid profileId, CancellationToken ct) =>
        db.WorkoutSessions.AsNoTracking().Where(s => s.ProfileId == profileId).ToListAsync(ct);

    private static WorkoutSessionDto ToDto(WorkoutSession s) => new(
        s.Date, s.Title,
        s.Sets.OrderBy(x => x.SetNumber).Select(x => new SetLogDto(x.ExerciseName, x.SetNumber, x.Reps, x.WeightKg)).ToList());
}
