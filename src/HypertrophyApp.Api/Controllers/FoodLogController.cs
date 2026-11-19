using HypertrophyApp.Application.Nutrition;
using HypertrophyApp.Shared.Nutrition;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

[Route("api/profiles/{profileId:guid}/foods")]
public sealed class FoodLogController(FoodLogService foods) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<IReadOnlyList<FoodEntryDto>>> GetDay(Guid profileId, [FromQuery] DateOnly date, CancellationToken ct) =>
        Ok(await foods.GetDayAsync(profileId, date, ct));

    [HttpPost]
    [ProducesResponseType<FoodEntryDto>(StatusCodes.Status200OK)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<FoodEntryDto>> Add(Guid profileId, AddFoodEntryRequest request, CancellationToken ct) =>
        ToActionResult(await foods.AddAsync(profileId, request, ct));

    [HttpDelete("{entryId:guid}")]
    public async Task<IActionResult> Delete(Guid profileId, Guid entryId, CancellationToken ct) =>
        await foods.DeleteAsync(profileId, entryId, ct) ? NoContent() : NotFound();

    /// <summary>Calorie and macro totals per day between two dates (inclusive, max 366 days).</summary>
    [HttpGet("daily-totals")]
    public async Task<ActionResult<IReadOnlyList<DailyIntakeDto>>> GetDailyTotals(
        Guid profileId, [FromQuery] DateOnly from, [FromQuery] DateOnly to, CancellationToken ct) =>
        to < from || to.DayNumber - from.DayNumber > 366
            ? Problem(detail: "Choose a range of up to one year.", statusCode: StatusCodes.Status400BadRequest)
            : Ok(await foods.GetDailyTotalsAsync(profileId, from, to, ct));
}
