using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class TaskDto
{
    public int Id { get; set; }

    [Required]
    [StringLength(200)]
    public string Title { get; set; } = null!;

    [Required]
    [StringLength(2000)]
    public string Description { get; set; } = null!;

    [Required]
    public DateTime DueDate { get; set; }

    [Required]
    [Range(1, 3)]
    public int Priority { get; set; }

    [Required]
    public int UserId { get; set; }

    public List<TagDto> Tags { get; set; } = new();
}