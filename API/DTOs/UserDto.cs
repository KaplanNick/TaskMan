namespace API.DTOs;

public class UserDto
{
    public int Id { get; set; }
    public string FullName { get; set; } = null!;
    public string Telephone { get; set; } = null!;
    public string Email { get; set; } = null!;
}