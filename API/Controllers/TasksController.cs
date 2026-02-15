using API.DTOs;
using API.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace API.Controllers;

public class TasksController : BaseApiController
{
    private readonly ITaskService _taskService;

    public TasksController(ITaskService taskService) => _taskService = taskService;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        var validationResult = ValidateModelState();
        if (validationResult != null)
            return validationResult;

        var result = await _taskService.CreateAsync(dto);
        return HandleResult(result, created => CreatedAtAction(nameof(GetById), new { id = created.Id }, created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var validationResult = ValidateId(id, "task");
        if (validationResult != null)
            return validationResult;

        var result = await _taskService.GetByIdAsync(id);
        return HandleResult(result, Ok);
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var result = await _taskService.GetAllAsync();
        return HandleResult(result, Ok);
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        var idValidation = ValidateId(id, "task");
        if (idValidation != null)
            return idValidation;

        var modelValidation = ValidateModelState();
        if (modelValidation != null)
            return modelValidation;

        var result = await _taskService.UpdateAsync(id, dto);
        return HandleResult(result, Ok);
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var validationResult = ValidateId(id, "task");
        if (validationResult != null)
            return validationResult;

        var result = await _taskService.DeleteAsync(id);
        return HandleResult(result, _ => NoContent());
    }
}
