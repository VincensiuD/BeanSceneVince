using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Models.Sitting
{
    public class Report
    {
        public int Id { get; set; }
        public string Name { get; set; }
        [Display(Name = "Start time")]
        public DateTime StartTime { get; set; }
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public SittingType SittingType { get; set; }
        [Display(Name = "Reservation Count")]
        public double ReservationCount { get; set; }
        [Display(Name = "Number of vacancies")]
        public int Vacanies { get; set; }
        [Display(Name = "Total people in sitting")]
        public double Pax { get; set; }
        public double PaxPecentage
        {
            get { return Math.Round((Pax / 40) * 100, 2); }
        }
        [Display(Name = "Number of tables reserved")]
        public int Tables { get; set; }
        [Display(Name = "Number of Bookings by Phone")]
        public double NumberOfPhoneBookings { get; set; }
        public double PhonePecentage
        {
            get { return Math.Round((NumberOfPhoneBookings / ReservationCount * 100), 2); }
        }
        [Display(Name = "Number of Bookings by Email")]
        public double NumberOfEmailBookings { get; set; }
        public double EmailPecentage
        {
            get { return Math.Round((NumberOfEmailBookings / ReservationCount * 100), 2); }
        }
        [Display(Name = "Number of Bookings Online")]
        public int NumberOfOnlineBookings { get; set; }
        public double OnlinePecentage
        {
            get { return Math.Round((NumberOfOnlineBookings / ReservationCount * 100), 2); }
        }
        [Display(Name = "Number of Walk in Bookings")]
        public double NumberOfWalkInBookings { get; set; }
        public double WalkInPecentage
        {
            get { return Math.Round((NumberOfWalkInBookings / ReservationCount * 100), 2); }
        }
    }
}
