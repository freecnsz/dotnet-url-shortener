using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using UrlShortener.Application.Common.Results;

namespace UrlShortener.Api.Extensions;

public static class ResultExtensions
{
    public static IActionResult ToActionResult<T>(this Result<T> result, ControllerBase controller)
        => result.IsSuccess
            ? controller.Ok(result.Value)
            : controller.ToProblem(result.Error);

    public static IActionResult ToCreatedResult<T>(
        this Result<T> result,
        ControllerBase controller,
        Func<T, string> locationSelector)
        => result.IsSuccess
            ? controller.Created(locationSelector(result.Value), result.Value)
            : controller.ToProblem(result.Error);

    public static IActionResult ToNoContentResult(this Result result, ControllerBase controller)
        => result.IsSuccess
            ? controller.NoContent()
            : controller.ToProblem(result.Error);

    private static ObjectResult ToProblem(this ControllerBase controller, Error error)
    {
        var status = error.Type switch
        {
            ErrorType.Validation => StatusCodes.Status400BadRequest,
            ErrorType.Unauthorized => StatusCodes.Status401Unauthorized,
            ErrorType.NotFound => StatusCodes.Status404NotFound,
            ErrorType.Gone => StatusCodes.Status410Gone,
            ErrorType.Conflict => StatusCodes.Status409Conflict,
            ErrorType.Failure => StatusCodes.Status500InternalServerError,
            _ => throw new UnreachableException($"Unmapped error type: {error.Type}")
        };

        var problem = controller.ProblemDetailsFactory.CreateProblemDetails(
            controller.HttpContext,
            statusCode: status,
            detail: error.Message);

        problem.Extensions["code"] = error.Code;

        return new ObjectResult(problem) { StatusCode = status };
    }
}
