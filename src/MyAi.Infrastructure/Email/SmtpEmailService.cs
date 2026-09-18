using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MyAi.Application.Common.Interfaces;
using System.Net;
using System.Net.Mail;

namespace MyAi.Infrastructure.Email;

/// <summary>
/// SMTP email sender. When Email:Enabled=false (default in development) emails are
/// only logged instead of sent — graceful degradation, no crash on missing SMTP.
/// </summary>
public class SmtpEmailService : IEmailService
{
    private readonly EmailConfiguration _configuration;

    private readonly ILogger<SmtpEmailService> _logger;

    public SmtpEmailService(IOptions<EmailConfiguration> configuration, ILogger<SmtpEmailService> logger)
    {
        _configuration = configuration.Value;
        _logger = logger;
    }

    public async Task SendAsync(string to, string subject, string htmlBody, CancellationToken ct = default)
    {
        if (!_configuration.Enabled)
        {
            _logger.LogInformation("[email disabled] To={To} Subject={Subject}", to, subject);
            return;
        }

        try
        {
            using var client = new SmtpClient(_configuration.SmtpHost, _configuration.SmtpPort)
            {
                EnableSsl = _configuration.UseSsl,
                Credentials = new NetworkCredential(_configuration.SmtpUsername, _configuration.SmtpPassword)
            };

            using var message = new MailMessage(
                new MailAddress(_configuration.FromAddress, _configuration.FromName),
                new MailAddress(to))
            {
                Subject = subject,
                Body = htmlBody,
                IsBodyHtml = true
            };

            await client.SendMailAsync(message, ct);
        }
        catch (Exception ex)
        {
            // Email must never break the main flow (e.g. registration).
            _logger.LogError(ex, "Failed to send email to {To}: {Message}", to, ex.Message);
        }
    }
}
