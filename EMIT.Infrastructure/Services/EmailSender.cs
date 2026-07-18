using System.Net;
using System.Net.Mail;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace EMIT.Infrastructure.Services;

public class EmailSender : IEmailSender<Data.ApplicationUser>
{
    private readonly ILogger<EmailSender> _logger;
    private readonly IConfiguration _configuration;

    public EmailSender(ILogger<EmailSender> logger, IConfiguration configuration)
    {
        _logger = logger;
        _configuration = configuration;
    }

    public async Task SendEmailAsync(string email, string subject, string htmlMessage)
    {
        var useConsole = _configuration.GetValue<bool>("EmailSettings:UseConsole");
        var smtpHost = _configuration["EmailSettings:SmtpHost"] ?? "";
        var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 587);
        var smtpUsername = _configuration["EmailSettings:SmtpUsername"] ?? "";
        var smtpPassword = _configuration["EmailSettings:SmtpPassword"] ?? "";
        var fromEmail = _configuration["EmailSettings:FromEmail"] ?? "noreply@emit-university.com";
        var fromName = _configuration["EmailSettings:FromName"] ?? "EMIT University";

        if (useConsole || string.IsNullOrWhiteSpace(smtpHost))
        {
            _logger.LogInformation("[EMAIL] To: {Email}", email);
            _logger.LogInformation("[EMAIL] Subject: {Subject}", subject);
            _logger.LogInformation("[EMAIL] Body: {Message}", htmlMessage);
            return;
        }

        try
        {
            using var message = new MailMessage();
            message.From = new MailAddress(fromEmail, fromName);
            message.To.Add(email);
            message.Subject = subject;
            message.Body = htmlMessage;
            message.IsBodyHtml = true;

            using var client = new SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new NetworkCredential(smtpUsername, smtpPassword),
                EnableSsl = true
            };

            await client.SendMailAsync(message);
            _logger.LogInformation("Email sent to {Email}: {Subject}", email, subject);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}: {Subject}", email, subject);
        }
    }

    public async Task SendConfirmationLinkAsync(Data.ApplicationUser user, string email, string confirmationLink)
    {
        await SendEmailAsync(email, "Confirmez votre email",
            $@"<p>Bonjour {user.FullName},</p>
               <p>Confirmez votre email en cliquant sur le lien ci-dessous :</p>
               <p><a href='{confirmationLink}'>Confirmer mon email</a></p>");
    }

    public async Task SendPasswordResetCodeAsync(Data.ApplicationUser user, string email, string resetCode)
    {
        await SendEmailAsync(email, "Réinitialisation de mot de passe",
            $@"<p>Bonjour {user.FullName},</p>
               <p>Votre code de réinitialisation est : <strong>{resetCode}</strong></p>
               <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>");
    }

    public async Task SendPasswordResetLinkAsync(Data.ApplicationUser user, string email, string resetLink)
    {
        await SendEmailAsync(email, "Réinitialisation de mot de passe",
            $@"<p>Bonjour {user.FullName},</p>
               <p>Réinitialisez votre mot de passe en cliquant sur le lien ci-dessous :</p>
               <p><a href='{resetLink}'>Réinitialiser mon mot de passe</a></p>
               <p>Si vous n'avez pas demandé cette réinitialisation, ignorez cet email.</p>");
    }
}
