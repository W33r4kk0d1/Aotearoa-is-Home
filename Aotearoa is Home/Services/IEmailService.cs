namespace Aotearoa_is_Home.Services
{
    public interface IEmailService
        {
            Task SendStudentApprovalEmailAsync(
                string recipientEmail,
                string firstName,
                string temporaryPassword);

            Task SendStudentAccountCreatedEmailAsync(
                string recipientEmail,
                string firstName);
        }
}