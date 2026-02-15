using API.Data;
using API.DTOs;
using API.Entities;
using API.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace API.Services;

public class TaskService : BaseService, ITaskService
{
    public TaskService(AppDbContext db) : base(db)
    {
    }

    public async Task<ServiceResult<TaskDto>> CreateAsync(CreateTaskDto dto)
    {
        var validationResult = ValidateOrFail<TaskDto>(ValidateTaskDto(dto.Title, dto.Description, dto.DueDate));
        if (validationResult != null)
            return validationResult;

        var user = await Db.Users.FirstOrDefaultAsync(u => u.Id == dto.UserId);
        if (user is null)
            return ServiceResult<TaskDto>.Fail(ServiceErrorType.Validation, "User not found.");

        var tagValidationResult = ValidateOrFail<TaskDto>(await ValidateTagIds(dto.TagIds));
        if (tagValidationResult != null)
            return tagValidationResult;

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

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Tasks.Add(task);
            await Db.SaveChangesAsync();

            var created = await Db.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .FirstAsync(t => t.Id == task.Id);

            return MapToDto(created);
        }, "Failed to create task");
    }

    public async Task<ServiceResult<TaskDto>> GetByIdAsync(int id)
    {
        var task = await Db.Tasks
            .AsNoTracking()
            .Include(t => t.User)
            .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
            .FirstOrDefaultAsync(t => t.Id == id);

        return task is null
            ? ServiceResult<TaskDto>.NotFound()
            : ServiceResult<TaskDto>.Ok(MapToDto(task));
    }

    public async Task<ServiceResult<List<TaskDto>>> GetAllAsync()
    {
        return await ExecuteQueryAsync(async () =>
        {
            var tasks = await Db.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .OrderByDescending(t => t.Id)
                .Take(200)
                .ToListAsync();

            return tasks.Select(MapToDto).ToList();
        }, "Failed to retrieve tasks");
    }

    public async Task<ServiceResult<TaskDto>> UpdateAsync(int id, UpdateTaskDto dto)
    {
        var validationResult = ValidateOrFail<TaskDto>(ValidateTaskDto(dto.Title, dto.Description, dto.DueDate));
        if (validationResult != null)
            return validationResult;

        var task = await Db.Tasks
            .Include(t => t.TaskTags)
            .FirstOrDefaultAsync(t => t.Id == id);

        if (task is null)
            return ServiceResult<TaskDto>.NotFound();

        var tagValidationResult = ValidateOrFail<TaskDto>(await ValidateTagIds(dto.TagIds));
        if (tagValidationResult != null)
            return tagValidationResult;

        var tags = await GetTagsByIdsAsync(dto.TagIds);

        var userExists = await Db.Users.AnyAsync(u => u.Id == task.UserId);
        if (!userExists)
            return ServiceResult<TaskDto>.Fail(ServiceErrorType.Validation, "Assigned user no longer exists");

        task.Title = dto.Title.Trim();
        task.Description = dto.Description.Trim();
        task.DueDate = dto.DueDate;
        task.Priority = (TaskPriority)dto.Priority;

        task.TaskTags.Clear();
        foreach (var tag in tags)
            task.TaskTags.Add(new TaskTag { TagId = tag.Id });

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            await Db.SaveChangesAsync();

            var updated = await Db.Tasks
                .AsNoTracking()
                .Include(t => t.User)
                .Include(t => t.TaskTags).ThenInclude(tt => tt.Tag)
                .FirstAsync(t => t.Id == id);

            return MapToDto(updated);
        }, "Failed to update task");
    }

    public async Task<ServiceResult<bool>> DeleteAsync(int id)
    {
        var task = await Db.Tasks.FindAsync(id);
        if (task is null)
            return ServiceResult<bool>.NotFound();

        return await ExecuteDatabaseOperationAsync(async () =>
        {
            Db.Tasks.Remove(task);
            await Db.SaveChangesAsync();
            return true;
        }, "Failed to delete task");
    }

    private static string? ValidateTaskDto(string title, string description, DateTime dueDate)
    {
        if (string.IsNullOrWhiteSpace(title))
            return "Title cannot be empty or contain only whitespace.";

        if (string.IsNullOrWhiteSpace(description))
            return "Description cannot be empty or contain only whitespace.";

        var trimmedTitle = title.Trim();
        if (trimmedTitle.Length < 3)
            return "Title must be at least 3 characters after trimming whitespace.";

        var trimmedDescription = description.Trim();
        if (trimmedDescription.Length < 10)
            return "Description must be at least 10 characters after trimming whitespace.";

        if (dueDate.Date < DateTime.UtcNow.Date)
            return "Due date cannot be in the past.";

        if (dueDate.Date > DateTime.UtcNow.Date.AddYears(10))
            return "Due date cannot be more than 10 years in the future.";

        return null;
    }

    private async Task<string?> ValidateTagIds(List<int> tagIds)
    {
        if (tagIds == null || tagIds.Count == 0)
            return "At least one tag is required.";

        if (tagIds.Any(id => id <= 0))
            return "All tag IDs must be positive numbers.";

        var distinctIds = tagIds.Distinct().ToList();
        if (distinctIds.Count != tagIds.Count)
            return "Duplicate tag IDs are not allowed.";

        var existingTags = await Db.Tags
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
        return await Db.Tags
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
