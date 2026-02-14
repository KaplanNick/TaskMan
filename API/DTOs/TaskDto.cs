using API.Entities;

namespace API.DTOs;

public class TaskDto
{
    public int Id { get; set; }
    public string Title { get; set; } = null!;
    public string Description { get; set; } = null!;
    public DateTime DueDate { get; set; }
    public int Priority { get; set; }
    public int UserId { get; set; }
    public List<TagDto> Tags { get; set; } = new();
}