using SampleSecureWeb.Models;
using System.Threading.Tasks;

namespace SampleSecureWeb.Data
{
    public interface IUser
    {
        User Registration(User user);
        User? Login(User user);
        User? GetUserByUsername(string username);
        User? GetUserByEmail(string email);
        void UpdateUserPassword(User user);
        void ChangePassword(string username, string newPassword);
        string GenerateOtp(string username);
        Task SendOtp(string username, string otp);
        bool ValidateOtp(string username, string otp);
    }
}
