using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TagsController : BaseApiController
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
        var validationResult = ValidateId(id, "tag");
        if (validationResult != null)
            return validationResult;

        var result = await _tagService.GetByIdAsync(id);
        return HandleResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] TagDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null)
            return validationResult;

        var result = await _tagService.CreateAsync(dto);
        return HandleResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] TagDto dto)
    {
        var idValidation = ValidateId(id, "tag");
        if (idValidation != null)
            return idValidation;

        var modelValidation = ValidateModelState();
        if (modelValidation != null)
            return modelValidation;

        var result = await _tagService.UpdateAsync(id, dto);
        return HandleResult(result, Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var validationResult = ValidateId(id, "tag");
        if (validationResult != null)
            return validationResult;

        var result = await _tagService.DeleteAsync(id);
        return HandleResult(result, _ => NoContent());
    }
}
