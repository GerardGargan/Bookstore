using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Bookstore.Models
{
    public class ApplicationUser : IdentityUser
    {
        [Required]
        public int Name { get; set; }
        public string? StreetAddress { get; set; }
        public string? City { get; set; }
        public string? County {  get; set; }
        public string? PostalCode { get; set; }
    }
}
