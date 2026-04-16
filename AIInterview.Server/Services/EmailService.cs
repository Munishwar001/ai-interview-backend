using AIInterview.Application.Interface;
using AIInterview.Server.Models;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using System.Web;

namespace AIInterview.Server.Services
{
    public sealed class EmailService : IEmailService
    {
        private const string ForgotPasswordTemplateRelativePath = "Templates/ForgotPasswordTemplate.html";

        private readonly EmailSettings _settings;
        private readonly ILogger<EmailService> _logger;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public EmailService(IOptions<EmailSettings> settings, ILogger<EmailService> logger, IWebHostEnvironment webHostEnvironment)
        {
            _settings = settings.Value;
            _logger = logger;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task SendForgotPasswordEmail(string toEmail, string resetUrl, CancellationToken cancellationToken = default)
        {
            if (string.IsNullOrWhiteSpace(_settings.SmtpHost)
                || _settings.SmtpPort <= 0
                || string.IsNullOrWhiteSpace(_settings.FromEmail))
            {
                throw new InvalidOperationException("EmailSettings is missing required SMTP values.");
            }

            var htmlBody = BuildForgotPasswordHtml(toEmail, resetUrl);

            using var message = new MailMessage
            {
                From = new MailAddress(_settings.FromEmail, string.IsNullOrWhiteSpace(_settings.FromName) ? _settings.FromEmail : _settings.FromName),
                Subject = "Reset your password",
                Body = htmlBody,
                IsBodyHtml = true
            };

            message.To.Add(toEmail);

            using var client = new SmtpClient(_settings.SmtpHost, _settings.SmtpPort)
            {
                EnableSsl = _settings.EnableSsl,
                Credentials = string.IsNullOrWhiteSpace(_settings.Username)
                    ? CredentialCache.DefaultNetworkCredentials
                    : new NetworkCredential(_settings.Username, _settings.Password)
            };

            await client.SendMailAsync(message, cancellationToken);
            _logger.LogInformation("Forgot-password email sent to {Email}", toEmail);
        }

        private string BuildForgotPasswordHtml(string toEmail, string resetUrl)
        {
            var templatePath = Path.Combine(_webHostEnvironment.ContentRootPath, ForgotPasswordTemplateRelativePath);
            if (!File.Exists(templatePath))
            {
                throw new FileNotFoundException($"Forgot-password email template not found at path: {templatePath}");
            }

            var template = File.ReadAllText(templatePath);
            var safeEmail = WebUtility.HtmlEncode(toEmail);
            var safeUrl = WebUtility.HtmlEncode(resetUrl);

            // Extract code from URL for display (last 8 chars of code parameter)
            var resetCode = ExtractResetCode(resetUrl);

            return template
                .Replace("{{USER_EMAIL}}", safeEmail, StringComparison.Ordinal)
                .Replace("{{RESET_URL}}", safeUrl, StringComparison.Ordinal)
                .Replace("{{RESET_CODE}}", resetCode, StringComparison.Ordinal);
        }

        private static string ExtractResetCode(string resetUrl)
        {
            try
            {
                // Extract the 'code' parameter from the URL
                var uri = new Uri(resetUrl);
                var query = System.Web.HttpUtility.ParseQueryString(uri.Query);
                var code = query.Get("code") ?? "UNKNOWN";
                
                // Return last 8 characters for a shorter, more readable code
                return code.Length > 8 ? code.Substring(code.Length - 8) : code;
            }
            catch
            {
                return "RESET-CODE";
            }
        }
    }
}