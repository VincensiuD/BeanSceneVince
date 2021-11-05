using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Models.Employee
{
    public class Update 
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Must contain more than 2 characters", MinimumLength = 2)]
        [Display(Name = "First Name")]
        public string FirstName { get; set; }

        [Required]
        [StringLength(100, ErrorMessage = "Must contain more than 2 characters", MinimumLength = 2)]
        [Display(Name = "Last Name")]
        public string LastName { get; set; }

        [Required]
        [Phone]
        [StringLength(11, ErrorMessage = "Must be a Valid Phone Number", MinimumLength = 10)]
        [Display(Name = "Phone Number")]
        public string Phone { get; set; }

        [Required]
        [EmailAddress]
        [Display(Name = "Email")]
        public string Email { get; set; }

        public string UserId { get; set; }
    }
}
