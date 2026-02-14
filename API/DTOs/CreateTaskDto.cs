namespace API.DTOs;

public class CreateTaskDto
{
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public int UserId { get; set; }
    public List<int> TagIds { get; set; } = new();
}