using System.Net;
using System.Net.Mail;

namespace Aotearoa_is_Home.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public async Task SendStudentApprovalEmailAsync(
            string recipientEmail,
            string firstName,
            string temporaryPassword)
        {
            var smtpHost = _configuration["Email:SmtpHost"];
            var smtpPort = int.Parse(
                _configuration["Email:SmtpPort"] ?? "587");

            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException(
                    "Email settings are not configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail,
                "Aotearoa is Home");

            message.To.Add(
                new MailAddress(recipientEmail));

            message.Subject =
                "Your Aotearoa is Home account has been approved";

            message.IsBodyHtml = true;

            message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
</head>

<body style='margin:0; padding:0; background:#f4f8f5; font-family:Arial, Helvetica, sans-serif;'>

    <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; overflow:hidden; border:1px solid #dfeae2;'>

        <div style='background:#397957; padding:28px; color:#ffffff;'>
            <h1 style='margin:0; font-size:26px;'>
                Aotearoa is Home
            </h1>

            <p style='margin:8px 0 0;'>
                Student Account Approved
            </p>
        </div>

        <div style='padding:32px;'>

            <p style='font-size:16px; color:#333333;'>
                Kia ora {WebUtility.HtmlEncode(firstName)},
            </p>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                Your student registration for
                <strong>Aotearoa is Home</strong>
                has been approved.
            </p>

            <div style='background:#f2f7f3; border:1px solid #d9e8dd; border-radius:10px; padding:20px; margin:25px 0;'>

                <p style='margin:0 0 12px; color:#52645a;'>
                    <strong>Login email</strong>
                </p>

                <p style='margin:0 0 18px; color:#173d2b;'>
                    {WebUtility.HtmlEncode(recipientEmail)}
                </p>

                <p style='margin:0 0 12px; color:#52645a;'>
                    <strong>Temporary password</strong>
                </p>

                <p style='margin:0; font-family:monospace; font-size:16px; color:#173d2b;'>
                    {WebUtility.HtmlEncode(temporaryPassword)}
                </p>

            </div>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                Please use these details to sign in to your account.
            </p>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                For security, please change your password after signing in.
            </p>

            <p style='margin-top:30px; color:#444444;'>
                Ngā mihi,<br>
                <strong>Aotearoa is Home Team</strong>
            </p>

        </div>

    </div>

</body>
</html>";

            using var smtpClient = new SmtpClient(
                smtpHost,
                smtpPort);

            smtpClient.EnableSsl = true;

            smtpClient.Credentials =
                new NetworkCredential(
                    username,
                    password);

            await smtpClient.SendMailAsync(message);
        }


        // -------------------------------------------------
        // ACTIVE STUDENT ACCOUNT CREATED EMAIL
        // -------------------------------------------------

        public async Task SendStudentAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName)
        {
            var smtpHost = _configuration["Email:SmtpHost"];

            var smtpPort = int.Parse(
                _configuration["Email:SmtpPort"] ?? "587");

            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException(
                    "Email settings are not configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail,
                "Aotearoa is Home");

            message.To.Add(
                new MailAddress(recipientEmail));

            message.Subject =
                "Your Aotearoa is Home account has been created";

            message.IsBodyHtml = true;

            message.Body = $@"
<!DOCTYPE html>
<html>
<head>
    <meta charset='utf-8'>
</head>

<body style='margin:0; padding:0; background:#f4f8f5; font-family:Arial, Helvetica, sans-serif;'>

    <div style='max-width:600px; margin:40px auto; background:#ffffff; border-radius:12px; overflow:hidden; border:1px solid #dfeae2;'>

        <div style='background:#397957; padding:28px; color:#ffffff;'>
            <h1 style='margin:0; font-size:26px;'>
                Aotearoa is Home
            </h1>

            <p style='margin:8px 0 0;'>
                Student Account Created
            </p>
        </div>

        <div style='padding:32px;'>

            <p style='font-size:16px; color:#333333;'>
                Kia ora {WebUtility.HtmlEncode(firstName)},
            </p>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                Your Student ID has been successfully verified and
                your <strong>Aotearoa is Home</strong> account has
                been created.
            </p>

            <div style='background:#f2f7f3; border:1px solid #d9e8dd; border-radius:10px; padding:20px; margin:25px 0;'>

                <p style='margin:0 0 12px; color:#52645a;'>
                    <strong>Login email</strong>
                </p>

                <p style='margin:0; color:#173d2b;'>
                    {WebUtility.HtmlEncode(recipientEmail)}
                </p>

            </div>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                You can now sign in using the password you created
                during registration.
            </p>

            <p style='font-size:15px; line-height:1.6; color:#444444;'>
                You do not need to wait for administrator approval.
            </p>

            <p style='margin-top:30px; color:#444444;'>
                Ngā mihi,<br>
                <strong>Aotearoa is Home Team</strong>
            </p>

        </div>

    </div>

</body>
</html>";

            using var smtpClient = new SmtpClient(
                smtpHost,
                smtpPort);

            smtpClient.EnableSsl = true;

            smtpClient.Credentials =
                new NetworkCredential(
                    username,
                    password);

            await smtpClient.SendMailAsync(message);
        }
    }
}