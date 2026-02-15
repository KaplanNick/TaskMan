using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class UsersController : BaseApiController
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
        var validationResult = ValidateId(id, "user");
        if (validationResult != null)
            return validationResult;

        var result = await _userService.GetByIdAsync(id);
        return HandleResult(result, Ok);
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateUserDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null)
            return validationResult;

        var result = await _userService.CreateAsync(dto);
        return HandleResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUserDto dto)
    {
        var idValidation = ValidateId(id, "user");
        if (idValidation != null)
            return idValidation;

        var modelValidation = ValidateModelState();
        if (modelValidation != null)
            return modelValidation;

        var result = await _userService.UpdateAsync(id, dto);
        return HandleResult(result, Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var validationResult = ValidateId(id, "user");
        if (validationResult != null)
            return validationResult;

        var result = await _userService.DeleteAsync(id);
        return HandleResult(result, _ => NoContent());
    }
}
