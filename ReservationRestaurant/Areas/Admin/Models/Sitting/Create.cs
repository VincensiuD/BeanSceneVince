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
        public string Name { get; set; }
        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        [Display(Name="Start Time")]
        public string Time1 { get; set; }
        [Display(Name = "End Time")]
        public string Time2 { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }

        [Display(Name = "Sitting")]
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; } //breakfast lunch dinner

        public SelectList SittingTypeSL { get; set; }

        [Display(Name = "Amount of Days")]
        public int Amount { get; set; }
        [Display(Name = "Close for Booking")]
        public bool IsClosed { get; set; }

    }
}
