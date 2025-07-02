using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace ERP.Infrastructure.External
{
    public interface IEmailService
    {
        Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default);
        Task SendProjectNotificationAsync(string projectName, string clientName, string managerEmail, CancellationToken cancellationToken = default);
        Task SendTimeEntryApprovalAsync(string employeeName, string projectName, decimal hours, string managerEmail, CancellationToken cancellationToken = default);
    }

    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailAsync(string to, string subject, string body, CancellationToken cancellationToken = default)
        {
            // Placeholder implementation
            // In production, integrate with SendGrid, AWS SES, or similar service
            _logger.LogInformation("Sending email to {To} with subject: {Subject}", to, subject);

            // Simulate async operation
            await Task.Delay(100, cancellationToken);
        }

        public async Task SendProjectNotificationAsync(string projectName, string clientName, string managerEmail, CancellationToken cancellationToken = default)
        {
            var subject = $"New Project Created: {projectName}";
            var body = $"A new project '{projectName}' has been created for client '{clientName}'. Please review the project details.";

            await SendEmailAsync(managerEmail, subject, body, cancellationToken);
        }

        public async Task SendTimeEntryApprovalAsync(string employeeName, string projectName, decimal hours, string managerEmail, CancellationToken cancellationToken = default)
        {
            var subject = "Time Entry Pending Approval";
            var body = $"Employee {employeeName} has logged {hours} hours for project '{projectName}' that requires your approval.";

            await SendEmailAsync(managerEmail, subject, body, cancellationToken);
        }
    }
}