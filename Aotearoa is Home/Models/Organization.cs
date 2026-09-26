using System.ComponentModel.DataAnnotations;

namespace Aotearoa_is_Home.Models
{
    public class Organization
    {
        public int Id { get; set; }

        [Required]
        [StringLength(150)]
        public string Name { get; set; } = string.Empty;

        public ICollection<Employee> Employees { get; set; }
            = new List<Employee>();
    }
}