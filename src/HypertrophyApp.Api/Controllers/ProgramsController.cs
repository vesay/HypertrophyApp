using HypertrophyApp.Application.Programs;
using HypertrophyApp.Shared.Programs;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

public sealed class ProgramsController : ApiControllerBase
{
    /// <summary>Lists the available training splits, optionally filtered by training days per week.</summary>
    [HttpGet("splits")]
    public ActionResult<IReadOnlyList<SplitSummaryDto>> GetSplits([FromQuery] int? days) =>
        Ok(ProgramCatalog.GetSplits(days));
}
