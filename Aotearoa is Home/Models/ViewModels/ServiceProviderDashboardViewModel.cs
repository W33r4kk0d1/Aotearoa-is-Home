using System;
using System.Collections.Generic;

using System.Collections.Generic;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class ServiceProviderDashboardViewModel
    {
        public string ProviderName { get; set; } = string.Empty;

        public int TotalEvents { get; set; }

        public int UpcomingEvents { get; set; }

        public int PendingApprovalEvents { get; set; }

        public int ApprovedEvents { get; set; }

        public int PeopleInterested { get; set; }

        public int EventViews { get; set; }

        public List<Event> UpcomingEventList { get; set; } = new();
    }
}