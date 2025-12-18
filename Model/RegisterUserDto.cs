using System.ComponentModel.DataAnnotations;

public class RegisterUserDto
{
    [Required]
    [MaxLength(50)]
    public string UserName { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    [Required]
    [MinLength(6)]
    public string Password { get; set; }

   // public string Role { get; set; } = "User"; // Optional for admins
}

