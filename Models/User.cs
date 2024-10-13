using System;
using System.ComponentModel.DataAnnotations;

namespace SampleSecureWeb.Models;

public class User
{
    [Key]
    public string Username { get; set; } = null!;
    [Required(ErrorMessage = "Password is required.")]
    [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
    public string Password { get; set; } = null!;
    public string RoleName { get; set; } = null!;
}
