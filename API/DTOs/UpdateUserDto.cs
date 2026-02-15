using System.ComponentModel.DataAnnotations;

namespace API.DTOs;

public class UpdateUserDto
{
    [Required(ErrorMessage = "Full name is required.")]
    [StringLength(200, MinimumLength = 2, ErrorMessage = "Full name must be between 2 and 200 characters.")]
    public string FullName { get; set; } = null!;

    [Required(ErrorMessage = "Telephone is required.")]
    [Phone(ErrorMessage = "Invalid telephone number format.")]
    [StringLength(50, ErrorMessage = "Telephone cannot exceed 50 characters.")]
    public string Telephone { get; set; } = null!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    [StringLength(320, ErrorMessage = "Email cannot exceed 320 characters.")]
    public string Email { get; set; } = null!;
}