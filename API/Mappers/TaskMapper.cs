using API.DTOs;
using API.Entities;

namespace API.Mappers;

public static class TaskMapper
{
    public static TaskDto ToDto(Entities.Task task)
    {
        return new TaskDto
        {
            Id = task.Id,
            Title = task.Title,
            Description = task.Description,
            DueDate = task.DueDate,
            Priority = (int)task.Priority,
            UserId = task.UserId,
            Tags = task.TaskTags
                .Select(tt => TagMapper.ToDto(tt.Tag))
                .OrderBy(t => t.Name)
                .ToList()
        };
    }

    public static Entities.Task ToEntity(CreateTaskDto dto, int userId, List<Tag> tags)
    {
        var task = new Entities.Task
        {
            Title = dto.Title.Trim(),
            Description = dto.Description.Trim(),
            DueDate = dto.DueDate,
            Priority = (TaskPriority)dto.Priority,
            UserId = userId
        };

        foreach (var tag in tags)
        {
            task.TaskTags.Add(new TaskTag { TagId = tag.Id });
        }

        return task;
    }

    public static void UpdateEntity(Entities.Task task, UpdateTaskDto dto, List<Tag> tags)
    {
        task.Title = dto.Title.Trim();
        task.Description = dto.Description.Trim();
        task.DueDate = dto.DueDate;
        task.Priority = (TaskPriority)dto.Priority;

        task.TaskTags.Clear();
        foreach (var tag in tags)
        {
            task.TaskTags.Add(new TaskTag { TagId = tag.Id });
        }
    }

    public static List<TaskDto> ToDtoList(IEnumerable<Entities.Task> tasks)
    {
        return tasks.Select(ToDto).ToList();
    }
}
