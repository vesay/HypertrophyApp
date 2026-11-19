using HypertrophyApp.Application.Workouts;
using HypertrophyApp.Shared.Workouts;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

[Route("api/profiles/{profileId:guid}")]
public sealed class WorkoutLogController(WorkoutLogService workouts) : ApiControllerBase
{
    /// <summary>Recent logged sessions, newest first.</summary>
    [HttpGet("sessions")]
    public async Task<ActionResult<IReadOnlyList<WorkoutSessionSummaryDto>>> GetHistory(
        Guid profileId, [FromQuery] int take = 60, CancellationToken ct = default) =>
        Ok(await workouts.GetHistoryAsync(profileId, take, ct));

    /// <summary>The session logged on a date (yyyy-MM-dd), or 204 if nothing was logged.</summary>
    [HttpGet("sessions/{date}")]
    public async Task<ActionResult<WorkoutSessionDto>> GetSession(Guid profileId, DateOnly date, CancellationToken ct) =>
        await workouts.GetSessionAsync(profileId, date, ct) is { } session ? Ok(session) : NoContent();

    [HttpPut("sessions/{date}")]
    public async Task<ActionResult<WorkoutSessionDto>> SaveSession(
        Guid profileId, DateOnly date, SaveWorkoutSessionRequest request, CancellationToken ct) =>
        ToActionResult(await workouts.SaveSessionAsync(profileId, date, request, ct));

    [HttpDelete("sessions/{date}")]
    public async Task<IActionResult> DeleteSession(Guid profileId, DateOnly date, CancellationToken ct) =>
        await workouts.DeleteSessionAsync(profileId, date, ct) ? NoContent() : NotFound();

    [HttpGet("progress/exercises")]
    public async Task<ActionResult<IReadOnlyList<string>>> GetLoggedExercises(Guid profileId, CancellationToken ct) =>
        Ok(await workouts.GetLoggedExercisesAsync(profileId, ct));

    [HttpGet("progress")]
    public async Task<ActionResult<ExerciseProgressDto>> GetProgress(
        Guid profileId, [FromQuery] string exercise, CancellationToken ct) =>
        string.IsNullOrWhiteSpace(exercise)
            ? Problem(detail: "Query parameter 'exercise' is required.", statusCode: StatusCodes.Status400BadRequest)
            : Ok(await workouts.GetProgressAsync(profileId, exercise, ct));
}
