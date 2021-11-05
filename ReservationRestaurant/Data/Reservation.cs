using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace ReservationRestaurant.Data
{
    public class Reservation
    {
        public int Id { get; set; }
        public DateTime StartTime { get; set; }
        public int Duration { get; set; } // in miuntes
        public DateTime EndTime { get => StartTime.AddMinutes(Duration); }
        public int Guests { get; set; } 
        public List<Table> Tables { get; set; } = new List<Table>();
        public int ReservationStatusId { get; set; }
        public ReservationStatus ReservationStatus { get; set; }
        public int ReservationOriginId { get; set; }
        public ReservationOrigin ReservationOrigin { get; set; }
        public int SittingId { get; set; }
        public Sitting Sitting { get; set; }
        public int PersonId { get; set; }
        public Person Person { get; set; }
        public string SpecialRequirement {get; set;}
    }
}
