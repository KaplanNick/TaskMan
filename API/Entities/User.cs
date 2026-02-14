namespace API.Entities;

public class User
{
    public int Id { get; set; }

    public string FullName { get; set; } = null!;
    public string Telephone { get; set; } = null!;
    public string Email { get; set; } = null!;

    public List<Task> Tasks { get; set; } = new();
}