namespace Aotearoa_is_Home.Models.ViewModels
{
    public class AdminDashboardViewModel
    {
        public int TotalAccounts { get; set; }

        public int PendingVerifications { get; set; }

        public int ServiceProviders { get; set; }

        public int NewThisWeek { get; set; }

        public List<RecentUserViewModel> RecentUsers { get; set; }
            = new List<RecentUserViewModel>();
    }

    public class RecentUserViewModel
    {
        public string FullName { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public DateTime? CreatedAt { get; set; }
    }
}