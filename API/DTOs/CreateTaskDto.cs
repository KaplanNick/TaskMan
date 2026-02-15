using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class CreateTaskDto
{
    [Required(ErrorMessage = "Title is required.")]
    [StringLength(200, MinimumLength = 3, ErrorMessage = "Title must be between 3 and 200 characters.")]
    public string Title { get; set; } = null!;

    [Required(ErrorMessage = "Description is required.")]
    [StringLength(2000, MinimumLength = 10, ErrorMessage = "Description must be between 10 and 2000 characters.")]
    public string Description { get; set; } = null!;

    [Required(ErrorMessage = "Due date is required.")]
    public DateTime DueDate { get; set; }

    [Required(ErrorMessage = "Priority is required.")]
    [Range(1, 3, ErrorMessage = "Priority must be between 1 (Low) and 3 (High).")]
    public int Priority { get; set; }

    [Required(ErrorMessage = "User ID is required.")]
    [Range(1, int.MaxValue, ErrorMessage = "User ID must be a positive number.")]
    public int UserId { get; set; }

    [Required(ErrorMessage = "At least one tag is required.")]
    [MinLength(1, ErrorMessage = "At least one tag is required.")]
    [MaxLength(10, ErrorMessage = "Cannot assign more than 10 tags.")]
    public List<int> TagIds { get; set; } = new();
}