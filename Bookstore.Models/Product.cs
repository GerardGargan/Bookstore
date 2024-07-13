using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
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
        [Display(Name = "List Price")]
        public int ListPrice { get; set; }

        [Required]
        [Display(Name = "Price for 1-50")]
        public int Price { get; set; }

        [Required]
        [Display(Name = "Price for 50+")]
        public int Price50 { get; set; }

        [Required]
        [Display(Name = "Price for 100+")]
        public int Price100 { get; set; }





    }
}
