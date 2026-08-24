namespace Aotearoa_is_Home.Models
{
    public class ContentBlock
    {
        public int Id { get; set; }

        public int SettlementPageId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int DisplayOrder { get; set; }

        public SettlementPage? SettlementPage { get; set; }
    }
}