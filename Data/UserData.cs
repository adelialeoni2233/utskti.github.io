using Microsoft.Extensions.Configuration;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using SampleSecureWeb.Models;
using System;
using System.Linq;

namespace SampleSecureWeb.Data
{
    public class UserData : IUser
    {
        private readonly ApplicationDbContext _context;
        private readonly string _smtpServer;
        private readonly int _smtpPort;
        private readonly string _senderEmail;
        private readonly string _senderUsername;
        private readonly string _senderPassword;
        private readonly bool _enableSsl;

        public UserData(ApplicationDbContext context, IConfiguration configuration)
        {
            _context = context;

            // Mengambil konfigurasi SMTP dari EmailSettings di appsettings.json
            _smtpServer = configuration["EmailSettings:SmtpServer"] ?? throw new InvalidOperationException("SMTP server configuration missing");
            _smtpPort = int.TryParse(configuration["EmailSettings:SmtpPort"], out var port) ? port : 25;
            _senderEmail = configuration["EmailSettings:SenderEmail"] ?? throw new InvalidOperationException("Sender email configuration missing");
            _senderUsername = configuration["EmailSettings:SenderUsername"] ?? throw new InvalidOperationException("Sender username configuration missing");
            _senderPassword = configuration["EmailSettings:SenderPassword"] ?? throw new InvalidOperationException("Sender password configuration missing");
            _enableSsl = bool.TryParse(configuration["EmailSettings:EnableSsl"], out var ssl) && ssl;
        }

        public User Registration(User user)
        {
            _context.Users.Add(user);
            _context.SaveChanges();
            return user;
        }

        public User? Login(User user)
        {
            var loginUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            return (loginUser != null && BCrypt.Net.BCrypt.Verify(user.Password, loginUser.Password)) ? loginUser : null;
        }

        public User? GetUserByUsername(string username)
        {
            return _context.Users.FirstOrDefault(u => u.Username == username);
        }

        public User? GetUserByEmail(string email)
        {
            return _context.Users.FirstOrDefault(u => u.Email == email);
        }

        public void UpdateUserPassword(User user)
        {
            var existingUser = _context.Users.FirstOrDefault(u => u.Username == user.Username);
            if (existingUser != null)
            {
                existingUser.Password = user.Password;
                _context.SaveChanges();
            }
        }

        public void ChangePassword(string username, string newPassword)
        {
            var user = GetUserByUsername(username);
            if (user != null)
            {
                user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
                _context.SaveChanges();
            }
        }

        public string GenerateOtp(string username)
        {
            var user = GetUserByUsername(username);
            if (user != null)
            {
                var otp = new Random().Next(100000, 999999).ToString();
                user.OtpCode = otp;
                user.OtpExpiry = DateTime.Now.AddMinutes(5); // OTP berlaku selama 5 menit
                _context.SaveChanges();
                return otp;
            }
            return string.Empty;
        }

        public async Task SendOtp(string username, string otp)
        {
            var user = GetUserByUsername(username);
            if (user == null) return;

            var subject = "Kode OTP Anda";
            var body = $"Kode OTP Anda adalah {otp}. Berlaku selama 5 menit.";

            await SendEmailAsync(user.Email, subject, body);
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            using (var client = new SmtpClient(_smtpServer, _smtpPort))
            {
                client.Credentials = new NetworkCredential(_senderUsername, _senderPassword);
                client.EnableSsl = _enableSsl;

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail),
                    Subject = subject,
                    Body = body,
                    IsBodyHtml = false
                };

                mailMessage.To.Add(toEmail);

                await client.SendMailAsync(mailMessage);
            }
        }

        public bool ValidateOtp(string username, string otp)
        {
            var user = GetUserByUsername(username);
            return user != null && user.OtpCode == otp && user.OtpExpiry > DateTime.Now;
        }
    }
}
