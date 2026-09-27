using Aotearoa_is_Home.Models;

namespace Aotearoa_is_Home.Models.ViewModels
{
    public class StudentHomeViewModel
    {
        public List<SettlementPage> SettlementPages { get; set; }
            = new List<SettlementPage>();

        public List<Event> Events { get; set; }
            = new List<Event>();
    }
}