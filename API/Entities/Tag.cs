namespace API.Entities;

public class Tag
{
    public int Id { get; set; }
    public string Name { get; set; } = null!;

    public ICollection<TaskTag> TaskTags { get; set; } = new List<TaskTag>();
}