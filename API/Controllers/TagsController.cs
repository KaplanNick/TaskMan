using API.DTOs;
using API.Interfaces;
using API.Services;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TagsController : ControllerBase
{
    private readonly ITagService _tagService;

    public TagsController(ITagService tagService)
    {
        _tagService = tagService;
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _tagService.GetAllAsync();
        return HandleResult(result, Ok);
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid tag ID." });

        var result = await _tagService.GetByIdAsync(id);
        return HandleResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagDto dto)
    {
        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var result = await _tagService.CreateAsync(dto);
        return HandleResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TagDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid tag ID." });

        if (!ModelState.IsValid)
        {
            var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
            return BadRequest(new { message = errors.FirstOrDefault() ?? "Invalid input" });
        }

        var result = await _tagService.UpdateAsync(id, dto);
        return HandleResult(result, Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "Invalid tag ID." });

        var result = await _tagService.DeleteAsync(id);
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
