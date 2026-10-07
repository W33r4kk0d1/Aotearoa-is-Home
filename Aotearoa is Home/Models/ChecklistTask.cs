using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class ChecklistTask
    {
        public int Id { get; set; }

        public int SettlementPageId { get; set; }

        [Required]
        public string Title { get; set; } = string.Empty;

        public string? Description { get; set; }

        public int DisplayOrder { get; set; }

        public SettlementPage? SettlementPage { get; set; }
    }
}