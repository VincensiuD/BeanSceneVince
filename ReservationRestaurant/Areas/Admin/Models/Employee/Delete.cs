using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Models.Employee
{
    public class Delete: Update
    {
        [Required]
        [Display(Name = "Delete From the System")]
        public bool FromTheSystem { get; set; }
    }
}
