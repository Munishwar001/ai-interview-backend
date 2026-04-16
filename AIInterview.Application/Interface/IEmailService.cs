namespace AIInterview.Application.Interface
{
    public interface IEmailService
    {
        Task SendForgotPasswordEmail(string toEmail, string resetUrl, CancellationToken cancellationToken = default);
    }
}