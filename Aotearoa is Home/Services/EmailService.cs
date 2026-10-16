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
            var smtpPort = int.Parse( _configuration["Email:SmtpPort"] ?? "587");

            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Email settings are not configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail,
                "Aotearoa is Home");

            message.To.Add(new MailAddress(recipientEmail));

            message.Subject = "Your Aotearoa is Home account has been approved";

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


        // ACTIVE STUDENT ACCOUNT CREATED EMAIL

        public async Task SendStudentAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName)
        {
            var smtpHost = _configuration["Email:SmtpHost"];

            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

            var username = _configuration["Email:Username"];
            var password = _configuration["Email:Password"];
            var fromEmail = _configuration["Email:FromEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Email settings are not configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail,
                "Aotearoa is Home");

            message.To.Add(new MailAddress(recipientEmail));

            message.Subject = "Your Aotearoa is Home account has been created";

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

        // -------------------------------------------------
        // ADMIN ACCOUNT CREATED EMAIL
        // -------------------------------------------------

        public async Task SendAdminAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName,
            string employeeId,
            string organisationName)
        {
            var smtpHost = _configuration["Email:SmtpHost"];

            var smtpPort = int.Parse(_configuration["Email:SmtpPort"] ?? "587");

            var username = _configuration["Email:Username"];

            var password = _configuration["Email:Password"];

            var fromEmail = _configuration["Email:FromEmail"];

            if (string.IsNullOrWhiteSpace(smtpHost) ||
                string.IsNullOrWhiteSpace(username) ||
                string.IsNullOrWhiteSpace(password) ||
                string.IsNullOrWhiteSpace(fromEmail))
            {
                throw new InvalidOperationException("Email settings are not configured.");
            }

            using var message = new MailMessage();

            message.From = new MailAddress(
                fromEmail,
                "Aotearoa is Home");

            message.To.Add(
                new MailAddress(recipientEmail));

            message.Subject = "Your Aotearoa is Home administrator account has been created";

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
                                    Administrator Account Created
                                </p>

                            </div>

                            <div style='padding:32px;'>

                                <p style='font-size:16px; color:#333333;'>
                                    Kia ora {WebUtility.HtmlEncode(firstName)},
                                </p>

                                <p style='font-size:15px; line-height:1.6; color:#444444;'>
                                    Your administrator registration for
                                    <strong>Aotearoa is Home</strong>
                                    has been successfully completed.
                                </p>

                                <p style='font-size:15px; line-height:1.6; color:#444444;'>
                                    Your employee details were verified using the
                                    organisation's employee records.
                                </p>

                                <div style='background:#f2f7f3; border:1px solid #d9e8dd; border-radius:10px; padding:20px; margin:25px 0;'>

                                    <p style='margin:0 0 12px; color:#52645a;'>
                                        <strong>Employee ID</strong>
                                    </p>

                                    <p style='margin:0 0 18px; color:#173d2b;'>
                                        {WebUtility.HtmlEncode(employeeId)}
                                    </p>

                                    <p style='margin:0 0 12px; color:#52645a;'>
                                        <strong>Organisation</strong>
                                    </p>

                                    <p style='margin:0 0 18px; color:#173d2b;'>
                                        {WebUtility.HtmlEncode(organisationName)}
                                    </p>

                                    <p style='margin:0 0 12px; color:#52645a;'>
                                        <strong>Login email</strong>
                                    </p>

                                    <p style='margin:0; color:#173d2b;'>
                                        {WebUtility.HtmlEncode(recipientEmail)}
                                    </p>

                                </div>

                                <p style='font-size:15px; line-height:1.6; color:#444444;'>
                                    You can now sign in using your registered email address
                                    and the password you created during registration.
                                </p>

                                <p style='font-size:15px; line-height:1.6; color:#444444;'>
                                    Your administrator account has been assigned the
                                    <strong>Admin</strong> role.
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

        public async Task SendFamilyAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName)
        {
            var safeName = WebUtility.HtmlEncode(firstName);

            await SendNotificationEmailAsync(
                recipientEmail,
                "Welcome to Aotearoa is Home",
                $"""
                <h2>Kia ora {safeName},</h2>
                <p>Your Family Member account has been successfully created.</p>
                <p>You can now sign in and explore settlement information
                and resources for your family.</p>
                <p>Ngā mihi,<br><strong>Aotearoa is Home Team</strong></p>
                """);
        }

        public async Task SendServiceProviderAccountCreatedEmailAsync(
            string recipientEmail,
            string providerName)
        {
            var safeName = WebUtility.HtmlEncode(providerName);

            await SendNotificationEmailAsync(
                recipientEmail,
                "Service Provider Registration Received",
                $"""
                <h2>Kia ora {safeName},</h2>
                <p>Your service provider account has been created successfully.</p>
                <p>Your account is awaiting administrator approval.
                You will be able to access the applicable provider features
                once your account has been approved.</p>
                <p>Ngā mihi,<br><strong>Aotearoa is Home Team</strong></p>
                """);
        }

        public async Task SendEventInterestNotificationAsync(
            string recipientEmail,
            string providerName,
            string eventName)
        {
            var safeName = WebUtility.HtmlEncode(providerName);
            var safeEventName = WebUtility.HtmlEncode(eventName);

            await SendNotificationEmailAsync(
                recipientEmail,
                "Someone Is Interested in Your Event",
                $"""
                <h2>Kia ora {safeName},</h2>
                <p>Someone has expressed interest in your event:</p>
                <p><strong>{safeEventName}</strong></p>
                <p>Please sign in to Aotearoa is Home to view the
                available event information and responses.</p>
                <p>Ngā mihi,<br><strong>Aotearoa is Home Team</strong></p>
                """);
        }

        private async Task SendNotificationEmailAsync(
            string recipientEmail,
            string subject,
            string htmlBody)
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

            using var message = new MailMessage
            {
                From = new MailAddress(fromEmail, "Aotearoa is Home"),
                Subject = subject,
                Body = $"""
                <!DOCTYPE html>
                <html>
                <body style="font-family:Arial,sans-serif;
                            color:#333;line-height:1.6;">
                    <div style="max-width:600px;margin:auto;padding:24px;">
                        <h1 style="color:#397957;">Aotearoa is Home</h1>
                        {htmlBody}
                    </div>
                </body>
                </html>
                """,
                IsBodyHtml = true
            };

            message.To.Add(new MailAddress(recipientEmail));

            using var smtpClient = new SmtpClient(smtpHost, smtpPort)
            {
                EnableSsl = true,
                Credentials = new NetworkCredential(username, password)
            };

            await smtpClient.SendMailAsync(message);
        }

        public async Task SendServiceProviderApprovalEmailAsync(
            string recipientEmail,
            string providerName)
        {
            var safeName = WebUtility.HtmlEncode(providerName);

            await SendNotificationEmailAsync(
                recipientEmail,
                "Your Aotearoa is Home Service Provider Account Has Been Approved",
                $"""
                <h2>Kia ora {safeName},</h2>
                <p>Great news! Your Service Provider account has been approved by the Aotearoa is Home administrator.</p>
                <p>You can now sign in and access the features available to approved service providers.</p>
                <p>Ngā mihi,<br>
                <strong>Aotearoa is Home Team</strong></p>
                """);
        }

        public async Task SendEmailConfirmationAsync(
            string recipientEmail,
            string firstName,
            string confirmationLink)
        {
            var safeName = WebUtility.HtmlEncode(firstName);
            var safeLink = WebUtility.HtmlEncode(confirmationLink);

            await SendNotificationEmailAsync(
                recipientEmail,
                "Verify Your Aotearoa is Home Email Address",
                $"""
                <h2>Kia ora {safeName},</h2>

                <p>Thank you for registering with Aotearoa is Home.</p>

                <p>Please click the link below to verify your email address:</p>

                <p>
                    <a href="{safeLink}">Verify my email address</a>
                </p>

                <p>If you did not create this account, you can ignore this email.</p>

                <p>Ngā mihi,<br>
                <strong>Aotearoa is Home Team</strong></p>
                """
            );
        }
    }
}