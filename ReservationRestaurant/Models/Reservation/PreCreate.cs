using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Models.Reservation
{
    public class PreCreate
    {
        [Display(Name = "Date")]
        [Required(ErrorMessage = "Please Select Date")]
        public string StartTime { get; set; }
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be at least {1}.")]
        public int Guests { get; set; } = 1;
        public SelectList SittingTypeSL { get; set; }
        [Display(Name = "Sitting")]
        [Required(ErrorMessage = "Please Select the Sitting")]
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; }
        public string Message { get; set; }
        public string AmountOfDaysForCalendar { get; set; }
    }
}