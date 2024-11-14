using System.ComponentModel.DataAnnotations;

namespace SampleSecureWeb.ViewModels
{
    public class RegistrationViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        [StringLength(50, ErrorMessage = "Username cannot exceed 50 characters.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Please enter a valid email address.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password is required.")]
        [StringLength(100, MinimumLength = 12, ErrorMessage = "Password must be at least 12 characters long.")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[\W_]).+$", ErrorMessage = "Password must contain at least one lowercase letter, one uppercase letter, one number, and one special character.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }

        [Required(ErrorMessage = "Please confirm your password.")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm password")]
        [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
        public string? ConfirmPassword { get; set; }

        public string? PasswordStrength
        {
            get
            {
                if (string.IsNullOrEmpty(Password)) return "Weak";

                int score = 0;
                if (Password.Length >= 12) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[A-Z]")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[a-z]")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"\d")) score++;
                if (System.Text.RegularExpressions.Regex.IsMatch(Password, @"[\W_]")) score++;

                return score switch
                {
                    5 => "Very Strong",
                    4 => "Strong",
                    3 => "Medium",
                    2 => "Weak",
                    _ => "Very Weak"
                };
            }
        }
    }
}
