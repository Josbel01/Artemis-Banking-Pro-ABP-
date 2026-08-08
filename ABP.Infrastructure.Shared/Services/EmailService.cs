using ABP.Core.Application.Dtos.Email;
using ABP.Core.Application.Interfaces;
using ABP.Core.Domain.Settings;
using MailKit.Security;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MimeKit;
using SmtpClient = MailKit.Net.Smtp.SmtpClient;

namespace ABP.Infrastructure.Shared.Services
{
    public class EmailService : IEmailService
    {
        private readonly EmailSettings _emailSettings;
        private readonly ILogger<EmailService> _logger;
        public EmailService(IOptions<EmailSettings> emailSettings, ILogger<EmailService> logger)
        {
            _emailSettings = emailSettings.Value;
            _logger = logger;
        }
        public async Task SendAsync(EmailRequestDto EmailRequestDto)
        {
            try
            {
                MimeMessage email = new()
                {
                    Subject = EmailRequestDto.Subject
                };

                email.From.Add(new MailboxAddress(_emailSettings.DisplayName ?? "", _emailSettings.EmailFrom));

                if (!string.IsNullOrWhiteSpace(EmailRequestDto.To))
                {
                    email.To.Add(MailboxAddress.Parse(EmailRequestDto.To));
                }

                if (EmailRequestDto.ToRange != null)
                {
                    foreach (var toItem in EmailRequestDto.ToRange)
                    {
                        if (!string.IsNullOrWhiteSpace(toItem))
                        {
                            email.To.Add(MailboxAddress.Parse(toItem));
                        }
                    }
                }

                BodyBuilder builder = new()
                {
                    HtmlBody = EmailRequestDto.HtmlBody
                };
                email.Body = builder.ToMessageBody();

                using var smtp = new SmtpClient();
                await smtp.ConnectAsync(_emailSettings.SmtpHost, _emailSettings.SmtpPort, SecureSocketOptions.StartTls);
                await smtp.AuthenticateAsync(_emailSettings.SmtpUser, _emailSettings.SmtpPass);
                await smtp.SendAsync(email);
                await smtp.DisconnectAsync(true);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al enviar el correo electronico");
            }
        }


    }
}
