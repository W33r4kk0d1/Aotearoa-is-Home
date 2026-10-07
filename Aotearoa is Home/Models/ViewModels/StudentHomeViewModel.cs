using Aotearoa_is_Home.Models;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class StudentHomeViewModel
    {
        public List<SettlementPage> SettlementPages { get; set; }
            = new List<SettlementPage>();

        public List<Event> Events { get; set; }
            = new List<Event>();

        public Dictionary<int, ChecklistProgressViewModel> ChecklistProgress { get; set; }
            = new Dictionary<int, ChecklistProgressViewModel>();
    }

    public class ChecklistProgressViewModel
    {
        public int Total { get; set; }

        public int Completed { get; set; }

        public int Remaining => Total - Completed;

        public int Percentage =>
            Total == 0
                ? 0
                : (Completed * 100) / Total;

        public string Status
        {
            get
            {
                if (Total == 0)
                {
                    return "No tasks";
                }

                if (Completed == Total)
                {
                    return "Completed";
                }

                if (Completed > 0)
                {
                    return "In progress";
                }

                return "Not started";
            }
        }
    }
}
