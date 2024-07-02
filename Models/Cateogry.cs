using System.ComponentModel.DataAnnotations;

namespace Bookstore.Models
{
    public class Cateogry
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Name { get; set; }
        public int DisplayOrder { get; set; }
    }
}
