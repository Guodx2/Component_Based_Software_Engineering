using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using SendGrid;
using SendGrid.Helpers.Mail;

namespace NKDUPVS_React.Server.Services
{
    /// <summary>
    /// Interface for email services, defining a method for sending emails asynchronously. This interface allows for different implementations of email sending functionality, such as using SendGrid or other email providers, while providing a consistent method signature for sending emails throughout the application.
    /// </summary>
    public interface IEmailService
    {
        /// <summary>
        /// Sends an email asynchronously to the specified recipient with the given subject, plain text content, and HTML content. This method is used to send various types of emails, such as account verification, password reset, or notifications, by providing the necessary email details and allowing for asynchronous execution to improve performance and responsiveness of the application.
        /// </summary>
        /// <param name="toEmail">The email address of the recipient.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="plainText">The plain text content of the email.</param>
        /// <param name="htmlContent">The HTML content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        Task SendEmailAsync(string toEmail, string subject, string plainText, string htmlContent);
    }

    /// <summary>
    /// Implementation of the IEmailService interface using SendGrid to send emails. This class retrieves the SendGrid API key and sender information from the application configuration, and uses the SendGrid client to create and send emails based on the provided recipient, subject, and content. The SendEmailAsync method constructs the email message and sends it asynchronously, allowing for efficient email delivery without blocking the main thread of the application.
    /// </summary>
    public class EmailService : IEmailService
    {
        private readonly string? _sendGridApiKey;
        private readonly string? _fromEmail;
        private readonly string? _fromName;

        /// <summary>
        /// Initializes a new instance of the EmailService class with the specified configuration. The constructor retrieves the SendGrid API key, sender email, and sender name from the application configuration, which are necessary for sending emails using the SendGrid service. This setup allows for flexible configuration of email sending parameters without hardcoding sensitive information in the codebase.
        /// </summary>
        /// <param name="configuration">The application configuration.</param>
        public EmailService(IConfiguration configuration)
        {
            _sendGridApiKey = configuration["SendGrid:ApiKey"];
            _fromEmail = configuration["SendGrid:FromEmail"];
            _fromName = configuration["SendGrid:FromName"];
        }

        /// <summary>
        /// Sends an email asynchronously to the specified recipient with the given subject, plain text content, and HTML content using the SendGrid service. This method constructs an email message using the sender information and recipient details, and then uses the SendGrid client to send the email. The asynchronous nature of this method allows for non-blocking email sending, improving the performance and responsiveness of the application when sending emails.
        /// </summary>
        /// <param name="toEmail">The email address of the recipient.</param>
        /// <param name="subject">The subject of the email.</param>
        /// <param name="plainText">The plain text content of the email.</param>
        /// <param name="htmlContent">The HTML content of the email.</param>
        /// <returns>A task representing the asynchronous operation.</returns>
        public async Task SendEmailAsync(string toEmail, string subject, string plainText, string htmlContent)
        {
            var client = new SendGridClient(_sendGridApiKey);
            var from = new EmailAddress(_fromEmail, _fromName);
            var to = new EmailAddress(toEmail);
            var msg = MailHelper.CreateSingleEmail(from, to, subject, plainText, htmlContent);
            await client.SendEmailAsync(msg);
        }
    }
}