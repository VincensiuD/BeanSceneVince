using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Data
{
    public class Sitting
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int Capacity { get; set; }
        public int RestaurantId { get; set; }
        public Restaurant Restaurant { get; set; }
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; } //breakfast lunch dinner
        public bool IsClosed { get; set; }
        //public int Guest { get; set; } // I don't need this prop
        public List<Reservation> Reservations { get; set; } = new List<Reservation>();
        // pax is the total number of guests
        public int Pax
        {
            get => Reservations.Sum(r => r.Guests);
        }
        //
        public int Vacancies
        {
            get
            {
               int vacancy= Capacity - Pax;
                if (vacancy < 0)
                {
                    return 0;
                }
                return vacancy;
            }
        }
    }
}
