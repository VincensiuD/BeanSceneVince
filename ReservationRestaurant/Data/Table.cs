using System.Collections.Generic;

namespace ReservationRestaurant.Data
{
    public class Table
    {
        public int Id { get; set; }
        public string Name { get; set; }

        public int AreaId { get; set; }
        public Area Area { get; set; }

        public List<Reservation> Reservations { get; set; } = new List<Reservation>();

    }
}