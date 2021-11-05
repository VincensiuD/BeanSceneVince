using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Models.Reservation
{
    public class Create
    {
        [Display(Name = "Start Time")]
        public String StartTime { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be at least {1}.")]
        public int Guests { get; set; }
        [Required(ErrorMessage = "Person - FirstName: Required")]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Person - LastName: Required")]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Person - Email: Required")]
        public string Email { get; set; }
        [Required(ErrorMessage = "Person - PhoneNumber: Required")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }
        public int PersonId { get; set; }
        [Display(Name = "Special Requirement")]
        public string SpecialRequirement { get; set; } = "None";
        [Display(Name = "Duration Time in minutes")]
        public int Duration { get; set; } = 60;
        [Display(Name = "Sitting Name")]
        [Required(ErrorMessage = "Rservation - Sitting Type: Required")]
        public int SittingId { get; set; }
        [Display(Name = "Reservation Origin")]
        public int ReservationOriginId { get; set; } = 3;// id=3 --> Online
        [Display(Name = "Reservation Status")]
        public int ReservationStatusId { get; set; } = 1;// id=1 --> Pending
        [Display(Name = "Sitting Type")]
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; }
        public SelectList TimeSL { get; set; }
        [Display(Name = "Start Time")]
        public string TimeSlot { get; set; }
      
        

    }
}

