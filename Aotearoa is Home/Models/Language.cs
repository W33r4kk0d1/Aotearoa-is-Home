using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class Language
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; } = string.Empty;

        public ICollection<ApplicationUser> Users { get; set; }
            = new List<ApplicationUser>();
    }
}