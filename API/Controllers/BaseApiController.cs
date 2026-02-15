using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public abstract class BaseApiController : ControllerBase
{
    /// <summary>
    /// Handles the ServiceResult and returns the appropriate IActionResult.
    /// </summary>
    /// <typeparam name="T">The type of the result value</typeparam>
    /// <param name="result">The service result to handle</param>
    /// <param name="onSuccess">Function to execute on success</param>
    /// <returns>An IActionResult based on the result</returns>
    protected IActionResult HandleResult<T>(ServiceResult<T> result, Func<T, IActionResult> onSuccess)
    {
        if (result.Success && result.Value != null)
            return onSuccess(result.Value);

        return result.ErrorType switch
        {
            ServiceErrorType.NotFound => NotFound(),
            ServiceErrorType.Validation => BadRequest(new { message = result.ErrorMessage ?? "Invalid input" }),
            ServiceErrorType.Database => StatusCode(500, new { message = result.ErrorMessage ?? "Database error" }),
            _ => StatusCode(500, new { message = result.ErrorMessage ?? "Unexpected error" })
        };
    }

    /// <summary>
    /// Validates if the ID is valid (greater than 0).
    /// </summary>
    /// <param name="id">The ID to validate</param>
    /// <param name="entityName">The entity name for the error message</param>
    /// <returns>A BadRequest result if invalid, otherwise null</returns>
    protected IActionResult? ValidateId(int id, string entityName)
    {
        if (id <= 0)
            return BadRequest(new { message = $"Invalid {entityName} ID." });
        
        return null;
    }

    /// <summary>
    /// Validates the ModelState and returns a BadRequest if invalid.
    /// </summary>
    /// <returns>A BadRequest result if invalid, otherwise null</returns>
    protected IActionResult? ValidateModelState()
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values
                .SelectMany(v => v.Errors)
                .Select(e => e.ErrorMessage)
                .ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        return null;
    }
}
