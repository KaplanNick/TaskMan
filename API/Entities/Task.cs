namespace API.Entities;

public class Task
{
    public int Id { get; set; }

    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }

    public TaskPriority Priority { get; set; }

    public int UserId { get; set; }
    public User User { get; set; } = null!;

    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}