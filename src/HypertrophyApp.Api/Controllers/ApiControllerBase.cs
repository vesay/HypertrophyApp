using HypertrophyApp.Application;
using Microsoft.AspNetCore.Mvc;

namespace HypertrophyApp.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class ApiControllerBase : ControllerBase
{
    /// <summary>Maps an application <see cref="Error"/> to an RFC 7807 ProblemDetails response.</summary>
    protected ActionResult Problem(Error error) => error.Type switch
    {
        ErrorType.NotFound => Problem(detail: error.Message, statusCode: StatusCodes.Status404NotFound),
        _ => Problem(detail: error.Message, statusCode: StatusCodes.Status400BadRequest)
    };

    protected ActionResult<T> ToActionResult<T>(Result<T> result) =>
        result.IsSuccess ? Ok(result.Value) : Problem(result.Error!);
}
