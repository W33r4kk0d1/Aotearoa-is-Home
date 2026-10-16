
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

        Task SendAdminAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName,
            string employeeId,
            string organisationName);

        Task SendEventInterestNotificationAsync(
            string recipientEmail,
            string providerName,
            string eventName);

        Task SendFamilyAccountCreatedEmailAsync(
            string recipientEmail,
            string firstName);

        Task SendServiceProviderAccountCreatedEmailAsync(
            string recipientEmail,
            string providerName);
        
        Task SendServiceProviderApprovalEmailAsync(
            string recipientEmail,
            string providerName);

        Task SendEmailConfirmationAsync(
            string recipientEmail,
            string firstName,
            string confirmationLink);
    }
}
