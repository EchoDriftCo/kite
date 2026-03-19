using System;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Resend;

namespace RecipeVault.DomainService {
    public class ResendEmailService : IEmailService {
        private readonly IResend resend;
        private readonly string fromEmail;
        private readonly ILogger<ResendEmailService> logger;

        public ResendEmailService(IResend resend, IConfiguration configuration, ILogger<ResendEmailService> logger) {
            this.resend = resend;
            this.fromEmail = configuration["Resend:FromEmail"];
            this.logger = logger;
        }

        public async Task SendCircleInviteAsync(string toEmail, string circleName, string inviterName, string inviteToken) {
            var inviteUrl = $"https://myrecipevault.io/circles/invite/{inviteToken}";
            var htmlBody = BuildInviteEmailHtml(circleName, inviterName, inviteUrl);

            try {
                var message = new EmailMessage {
                    From = fromEmail,
                    To = toEmail,
                    Subject = $"You've been invited to join {circleName} on RecipeVault",
                    HtmlBody = htmlBody
                };

                await resend.EmailSendAsync(message).ConfigureAwait(false);
                logger.LogInformation("Circle invite email sent to {Email} for circle {CircleName}", toEmail, circleName);
            } catch (Exception ex) {
                logger.LogError(ex, "Failed to send circle invite email to {Email} for circle {CircleName}", toEmail, circleName);
            }
        }

        private static string BuildInviteEmailHtml(string circleName, string inviterName, string inviteUrl) {
            return $@"<!DOCTYPE html>
<html>
<head>
  <meta charset=""utf-8"" />
  <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"" />
</head>
<body style=""margin:0; padding:0; background-color:#f4f4f5; font-family:-apple-system, BlinkMacSystemFont, 'Segoe UI', Roboto, sans-serif;"">
  <table width=""100%"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#f4f4f5; padding:40px 0;"">
    <tr>
      <td align=""center"">
        <table width=""560"" cellpadding=""0"" cellspacing=""0"" style=""background-color:#ffffff; border-radius:8px; overflow:hidden;"">
          <tr>
            <td style=""background-color:#16a34a; padding:32px; text-align:center;"">
              <h1 style=""margin:0; color:#ffffff; font-size:24px; font-weight:600;"">RecipeVault</h1>
            </td>
          </tr>
          <tr>
            <td style=""padding:40px 32px;"">
              <h2 style=""margin:0 0 16px; color:#18181b; font-size:20px; font-weight:600;"">You're invited!</h2>
              <p style=""margin:0 0 24px; color:#3f3f46; font-size:16px; line-height:1.5;"">
                <strong>{inviterName}</strong> has invited you to join the circle <strong>{circleName}</strong> on RecipeVault. Join to share and discover recipes together.
              </p>
              <table cellpadding=""0"" cellspacing=""0"" style=""margin:0 0 24px;"">
                <tr>
                  <td style=""background-color:#16a34a; border-radius:6px;"">
                    <a href=""{inviteUrl}"" style=""display:inline-block; padding:14px 32px; color:#ffffff; font-size:16px; font-weight:600; text-decoration:none;"">Accept Invitation</a>
                  </td>
                </tr>
              </table>
              <p style=""margin:0 0 8px; color:#71717a; font-size:13px; line-height:1.5;"">
                This invitation will expire in 7 days. If you didn't expect this email, you can safely ignore it.
              </p>
              <p style=""margin:0; color:#a1a1aa; font-size:12px; line-height:1.5; word-break:break-all;"">
                {inviteUrl}
              </p>
            </td>
          </tr>
          <tr>
            <td style=""padding:24px 32px; border-top:1px solid #e4e4e7; text-align:center;"">
              <p style=""margin:0; color:#a1a1aa; font-size:12px;"">RecipeVault &mdash; Share recipes with the people you love</p>
            </td>
          </tr>
        </table>
      </td>
    </tr>
  </table>
</body>
</html>";
        }
    }
}
