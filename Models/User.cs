using System;
using System.ComponentModel.DataAnnotations;

namespace SampleSecureWeb.Models
{
    public class User
    {
        [Key]
        public string Username { get; set; } = null!;

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
        public string Password { get; set; } = null!;

        public string RoleName { get; set; } = "User"; // Set nilai default ke "User"

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email format.")]
        public string Email { get; set; } = null!; // Properti Email untuk OTP

        // Properti tambahan untuk OTP
        public string? OtpCode { get; set; } // Menyimpan kode OTP sementara
        public DateTime? OtpExpiry { get; set; } // Menyimpan waktu kedaluwarsa OTP
    }
}
