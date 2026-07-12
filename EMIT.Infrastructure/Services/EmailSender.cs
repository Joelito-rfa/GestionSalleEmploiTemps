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

        if (useConsole)
        {
            _logger.LogInformation("[EMAIL] À: {Email}", email);
            _logger.LogInformation("[EMAIL] Sujet: {Subject}", subject);
            _logger.LogInformation("[EMAIL] Message: {Message}", htmlMessage);
        }

        await Task.CompletedTask;
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
