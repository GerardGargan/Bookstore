using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Description { get; set; }
        [Required]
        public string ISBN { get; set; }
        [Required]
        public string Author { get; set; }

        [Required]
        [Range(1, 1000)]
        [Display(Name = "List Price")]
        public int ListPrice { get; set; }

        [Range(1, 1000)]
        [Required]
        [Display(Name = "Price for 1-50")]
        public int Price { get; set; }

        [Range(1, 1000)]
        [Required]
        [Display(Name = "Price for 50+")]
        public int Price50 { get; set; }

        [Range(1, 1000)]
        [Required]
        [Display(Name = "Price for 100+")]
        public int Price100 { get; set; }

        public int CategoryId { get; set; }
        [ForeignKey("CategoryId")]
        [ValidateNever]
        public Category category { get; set; }
        [ValidateNever]
        public string ImageUrl { get; set; }

    }
}
