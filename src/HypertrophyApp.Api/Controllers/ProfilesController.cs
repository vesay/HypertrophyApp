using HypertrophyApp.Application.Profiles;
using HypertrophyApp.Shared.Dashboard;
using HypertrophyApp.Shared.Profiles;
using HypertrophyApp.Shared.Programs;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

public sealed class ProfilesController(ProfileService profiles) : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType<ProfileDto>(StatusCodes.Status201Created)]
    [ProducesResponseType<ProblemDetails>(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<ProfileDto>> Create(SaveProfileRequest request, CancellationToken ct)
    {
        var result = await profiles.CreateAsync(request, ct);
        return result.IsSuccess
            ? CreatedAtAction(nameof(Get), new { id = result.Value!.Id }, result.Value)
            : Problem(result.Error!);
    }

    [HttpGet("{id:guid}")]
    public async Task<ActionResult<ProfileDto>> Get(Guid id, CancellationToken ct) =>
        ToActionResult(await profiles.GetAsync(id, ct));

    [HttpPut("{id:guid}")]
    public async Task<ActionResult<ProfileDto>> Update(Guid id, SaveProfileRequest request, CancellationToken ct) =>
        ToActionResult(await profiles.UpdateAsync(id, request, ct));

    /// <summary>Permanently deletes the profile together with all of its logs.</summary>
    [HttpDelete("{id:guid}")]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct) =>
        await profiles.DeleteAsync(id, ct) ? NoContent() : NotFound();

    [HttpGet("{id:guid}/dashboard")]
    public async Task<ActionResult<DashboardDto>> GetDashboard(Guid id, CancellationToken ct) =>
        ToActionResult(await profiles.GetDashboardAsync(id, ct));

    [HttpGet("{id:guid}/custom-program")]
    public async Task<ActionResult<IReadOnlyList<CustomDayDto>>> GetCustomProgram(Guid id, CancellationToken ct) =>
        ToActionResult(await profiles.GetCustomProgramAsync(id, ct));

    /// <summary>Saves the user's own program and switches the profile to it.</summary>
    [HttpPut("{id:guid}/custom-program")]
    public async Task<ActionResult<ProfileDto>> SaveCustomProgram(Guid id, SaveCustomProgramRequest request, CancellationToken ct) =>
        ToActionResult(await profiles.SaveCustomProgramAsync(id, request, ct));
}
