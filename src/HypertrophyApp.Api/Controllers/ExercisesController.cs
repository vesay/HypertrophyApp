using HypertrophyApp.Application.Exercises;
using HypertrophyApp.Shared.Exercises;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

/// <summary>The exercise library: 870+ exercises with muscles, equipment, instructions and form demo images.</summary>
public sealed class ExercisesController(ExerciseLibraryService library) : ApiControllerBase
{
    [HttpGet]
    public async Task<ActionResult<ExerciseSearchResultDto>> Search(
        [FromQuery] string? q, [FromQuery] string? muscle, [FromQuery] string? equipment,
        [FromQuery] int skip = 0, [FromQuery] int take = 24, CancellationToken ct = default) =>
        Ok(await library.SearchAsync(q, muscle, equipment, skip, take, ct));

    [HttpGet("filters")]
    public async Task<ActionResult<ExerciseFiltersDto>> GetFilters(CancellationToken ct) =>
        Ok(await library.GetFiltersAsync(ct));

    [HttpGet("{id}")]
    public async Task<ActionResult<ExerciseDetailDto>> Get(string id, CancellationToken ct) =>
        await library.GetAsync(id, ct) is { } exercise ? Ok(exercise) : NotFound();
}
