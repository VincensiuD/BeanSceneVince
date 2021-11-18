using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Models.Sitting
{
    public class Create
    {
        [Required]
        public string Name { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be at least {1}.")]
        public int Capacity { get; set; } = 40;
        [Display(Name = "Start Time")]
        [Required]
        public string Time1 { get; set; }
        [Display(Name = "End Time")]
        [Required]
        public string Time2 { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        [Required]
        [Display(Name = "Sitting")]
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; } //breakfast lunch dinner
        public SelectList SittingTypeSL { get; set; }
        [Required]
        [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be at least {1}.")]
        [Display(Name = "Amount of Days")]
        public int Amount { get; set; }
        [Display(Name = "Close for Booking")]
        public bool IsClosed { get; set; }
    }
}


