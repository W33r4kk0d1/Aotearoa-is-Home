using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    // Stores public holidays and important calendar reminders.
    public class CalendarInformation
    {
        public int Id { get; set; }

        // Name displayed on the calendar.
        [Required]
        public string Title { get; set; } = string.Empty;

        // Additional information shown to users.
        public string? Description { get; set; }

        // Date of the holiday or reminder.
        [Required]
        public DateTime Date { get; set; }

        // Type of calendar information.
        // Examples: Public Holiday, Regional Holiday, Reminder.
        [Required]
        public string Type { get; set; } = string.Empty;

        // Indicates whether the item is a public holiday.
        public bool IsPublicHoliday { get; set; }

        // Indicates whether shops may be required to close.
        public bool IsShopClosure { get; set; }

        // Region for regional holidays.
        // Leave empty for national holidays.
        public string? Region { get; set; }
    }
}