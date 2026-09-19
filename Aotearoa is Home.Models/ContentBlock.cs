namespace Aotearoa_is_Home.Models
{
    public class ContentBlock
    {
        public int Id { get; set; }

        public int SettlementPageId { get; set; }

        public string Type { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public string Details { get; set; } = string.Empty;

        public byte[]? ImageData { get; set; }

        public string? ImageContentType { get; set; }

        public int DisplayOrder { get; set; }

        public SettlementPage? SettlementPage { get; set; }
    }
}