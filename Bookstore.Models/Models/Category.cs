using System.ComponentModel.DataAnnotations;

namespace Bookstore.Models
{
    public class Category
    {
        [Key]
        public int Id { get; set; }
        [Required]
        [MaxLength(50, ErrorMessage = "Name must be less than 50 characters")]
        [Display(Name = "Category Name")]
        public string Name { get; set; }
        [Display(Name = "Display Order")]
        [Range(1, 100, ErrorMessage = "Display Order must be between 1 and 100")]
        public int DisplayOrder { get; set; }
    }
}
