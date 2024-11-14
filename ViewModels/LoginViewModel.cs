using System.ComponentModel.DataAnnotations;

namespace SampleSecureWeb.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Username is required.")]
        public string? Username { get; set; } = ""; // Tambahkan ? untuk nullable dan inisialisasi

        [Required(ErrorMessage = "Password is required.")]
        [DataType(DataType.Password)]
        public string? Password { get; set; } = ""; // Tambahkan ? untuk nullable dan inisialisasi

        public string? ReturnUrl { get; set; }
        
        public bool RememberLogin { get; set; }
    }
}
