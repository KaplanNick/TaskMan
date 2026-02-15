using API.DTOs;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _userService.GetAllAsync();
        return HandleResult(result, Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        var result = await _userService.GetByIdAsync(id);
        return HandleResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var result = await _userService.CreateAsync(dto);
        return HandleResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var result = await _userService.UpdateAsync(id, dto);
        return HandleResult(result, Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid user ID." });

        var result = await _userService.DeleteAsync(id);
        return HandleResult(result, _ => NoContent());
    }

    private IActionResult HandleResult<T>(ServiceResult<T> result, Func<T, IActionResult> onSuccess)
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
}
