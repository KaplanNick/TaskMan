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
        // Find or create user
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user is null)
            return BadRequest("User not found.");

        // Get or create tags
        var tags = await GetOrCreateTagsAsync(dto.TagIds);
        if (tags.Count == 0)
            return BadRequest("At least one valid tag is required.");

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
        var task = await _db.Tasks
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return NotFound();

        // Get or create tags
        var tags = await GetOrCreateTagsAsync(dto.TagIds);
        if (tags.Count == 0)
            return BadRequest("At least one valid tag is required.");

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
        var task = await _db.Tasks.FindAsync(id);
        if (task is null)
            return NotFound();

        _db.Tasks.Remove(task);
        await _db.SaveChangesAsync();

        return NoContent();
    }

    private async Task<List<Tag>> GetOrCreateTagsAsync(List<int> tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
            return new List<Tag>();

        var distinctIds = tagIds.Distinct().ToList();
        var tags = await _db.Tags
            .Where(t => distinctIds.Contains(t.Id))
            .ToListAsync();

        return tags;
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