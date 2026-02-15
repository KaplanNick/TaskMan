using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Entities;
using API.DTOs;

namespace API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TasksController : ControllerBase
{
    private readonly AppDbContext _db;

    public TasksController(AppDbContext db) => _db = db;

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateTaskDto dto)
    {
        // Model validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Custom validations
        var validationError = ValidateTaskDto(dto.Title, dto.Description, dto.DueDate);
        if (validationError != null)
            return BadRequest(validationError);

        // Validate user exists
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user is null)
            return BadRequest("User not found.");

        // Validate tag IDs
        var tagValidationError = await ValidateTagIds(dto.TagIds);
        if (tagValidationError != null)
            return BadRequest(tagValidationError);

        // Get tags
        var tags = await GetTagsByIdsAsync(dto.TagIds);

        var task = new API.Entities.Task
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            DueDate = dto.DueDate,
            Priority = (TaskPriority)dto.Priority,
            UserId = user.Id
        };

        foreach (var tag in tags)
            task.TaskTags.Add(new TaskTag { TagId = tag.Id });

        _db.Tasks.Add(task);
        await _db.SaveChangesAsync();

        // Load full object for response
        var created = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstAsync(t => t.Id == task.Id);

        return CreatedAtAction(nameof(GetById), new { id = created.Id }, MapToDto(created));
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid task ID.");

        var task = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null ? NotFound() : Ok(MapToDto(task));
    }

    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        var tasks = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .OrderByDescending(t => t.Id)
            .Take(200)
            .ToListAsync();

        return Ok(tasks.Select(MapToDto));
    }

    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateTaskDto dto)
    {
        if (id <= 0)
            return BadRequest("Invalid task ID.");

        // Model validation
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        // Custom validations
        var validationError = ValidateTaskDto(dto.Title, dto.Description, dto.DueDate);
        if (validationError != null)
            return BadRequest(validationError);

        var task = await _db.Tasks
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return NotFound();

        // Validate tag IDs
        var tagValidationError = await ValidateTagIds(dto.TagIds);
        if (tagValidationError != null)
            return BadRequest(tagValidationError);

        // Get tags
        var tags = await GetTagsByIdsAsync(dto.TagIds);

        task.Title = dto.Title.Trim();
        task.Description = dto.Description.Trim();
        task.DueDate = dto.DueDate;
        task.Priority = (TaskPriority)dto.Priority;

        task.TaskTags.Clear();
        foreach (var tag in tags)
            task.TaskTags.Add(new TaskTag { TagId = tag.Id });

        await _db.SaveChangesAsync();

        var updated = await _db.Tasks
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstAsync(t => t.Id == id);

        return Ok(MapToDto(updated));
    }

    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        if (id <= 0)
            return BadRequest("Invalid task ID.");

        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private static string? ValidateTaskDto(string title, string description, DateTime dueDate)
    {
        // Check for whitespace-only strings
        if (string.IsNullOrWhiteSpace(title))
            return "Title cannot be empty or contain only whitespace.";

        if (string.IsNullOrWhiteSpace(description))
            return "Description cannot be empty or contain only whitespace.";

        // Trim and check actual length
        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length < 3)
            return "Title must be at least 3 characters after trimming whitespace.";

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < 10)
            return "Description must be at least 10 characters after trimming whitespace.";

        // Validate due date is not in the past
        if (dueDate.Date < DateTime.UtcNow.Date)
            return "Due date cannot be in the past.";

        // Validate due date is not too far in the future (e.g., 10 years)
        if (dueDate.Date > DateTime.UtcNow.Date.AddYears(10))
            return "Due date cannot be more than 10 years in the future.";

        return null;
    }

    private async Task<string?> ValidateTagIds(List<int> tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
            return "At least one tag is required.";

        // Check for invalid tag IDs (non-positive)
        if (tagIds.Any(id => id <= 0))
            return "All tag IDs must be positive numbers.";

        // Check for duplicate tag IDs
        var distinctIds = tagIds.Distinct().ToList();
        if (distinctIds.Count != tagIds.Count)
            return "Duplicate tag IDs are not allowed.";

        // Verify all tag IDs exist in database
        var existingTags = await _db.Tags
            .Where(t => distinctIds.Contains(t.Id))
            .ToListAsync();

        if (existingTags.Count != distinctIds.Count)
        {
            var missingIds = distinctIds.Except(existingTags.Select(t => t.Id)).ToList();
            return $"The following tag IDs do not exist: {string.Join(", ", missingIds)}";
        }

        return null;
    }

    private async Task<List<Tag>> GetTagsByIdsAsync(List<int> tagIds)
    {
        var distinctIds = tagIds.Distinct().ToList();
        return await _db.Tags
            .Where(t => distinctIds.Contains(t.Id))
            .ToListAsync();
    }

    private static TaskDto MapToDto(API.Entities.Task task) =>
        new()
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = (int)task.Priority,
            UserId = task.UserId,
            Tags = task.TaskTags
                .Select(tt => new TagDto { Id = tt.Tag.Id, Name = tt.Tag.Name })
                .OrderBy(t => t.Name)
                .ToList()
        };
}