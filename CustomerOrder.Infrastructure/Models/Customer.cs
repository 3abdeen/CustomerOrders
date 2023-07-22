using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CustomerOrder.Infrastructure.Models
{
    public class Customer : IdentityUser
    {
        [Required(AllowEmptyStrings = false, ErrorMessage = "Address Required.")]
        public string Address { get; set; }
    }
}
