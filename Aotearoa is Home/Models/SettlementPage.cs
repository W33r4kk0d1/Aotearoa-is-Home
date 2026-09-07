namespace Aotearoa_is_Home.Models
{
    public class SettlementPage
    {
        public int Id { get; set; }

    public string CategoryName { get; set; } = string.Empty;

        public byte[]? BackgroundImage { get; set; }

        public string? BackgroundImageContentType { get; set; }

        public List<ContentBlock> ContentBlocks { get; set; } = new List<ContentBlock>();
    }
}
