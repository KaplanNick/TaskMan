namespace API.Entities;

public class TaskTag
{
    public int TaskId { get; set; }
    public Task Task { get; set; } = null!;

    public int TagId { get; set; }
    public Tag Tag { get; set; } = null!;
    
}