using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace SampleSecureWeb.Services
{
    public class SmtpEmailService
    {
        private readonly string? _smtpServer;
        private readonly int _smtpPort;
        private readonly string? _senderEmail;
        private readonly string? _senderUsername;
        private readonly string? _senderPassword;
        private readonly bool _enableSsl;
        private readonly ILogger<SmtpEmailService> _logger;

        public SmtpEmailService(IConfiguration configuration, ILogger<SmtpEmailService> logger)
        {
            _smtpServer = configuration["EmailSettings:SmtpServer"];
            _senderEmail = configuration["EmailSettings:SenderEmail"];
            _senderUsername = configuration["EmailSettings:SenderUsername"];
            _senderPassword = configuration["EmailSettings:SenderPassword"];
            _enableSsl = bool.Parse(configuration["EmailSettings:EnableSsl"] ?? "true");

            // Gunakan int.TryParse untuk menghindari ArgumentNullException
            if (!int.TryParse(configuration["EmailSettings:SmtpPort"], out _smtpPort))
            {
                _smtpPort = 587; // Nilai default untuk port SMTP
            }

            _logger = logger;

            // Validasi konfigurasi SMTP
            if (string.IsNullOrEmpty(_smtpServer) || string.IsNullOrEmpty(_senderEmail) || 
                string.IsNullOrEmpty(_senderUsername) || string.IsNullOrEmpty(_senderPassword))
            {
                throw new InvalidOperationException("Konfigurasi SMTP tidak lengkap di appsettings.json.");
            }
        }

        public async Task<bool> SendEmailAsync(string toEmail, string subject, string message)
        {
            try
            {
                var mailMessage = new MailMessage
                {
                    From = new MailAddress(_senderEmail ?? "no-reply@example.com"), // fallback address
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = false
                };
                mailMessage.To.Add(toEmail);

                using (var smtpClient = new SmtpClient(_smtpServer, _smtpPort))
                {
                    smtpClient.Credentials = new NetworkCredential(_senderUsername ?? "", _senderPassword ?? "");
                    smtpClient.EnableSsl = _enableSsl;

                    await smtpClient.SendMailAsync(mailMessage);
                    return true;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError("Gagal mengirim email melalui SMTP. Error: {Error}", ex.Message);
                return false;
            }
        }
    }
}
