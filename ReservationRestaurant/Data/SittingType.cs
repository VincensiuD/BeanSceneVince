using System.Collections.Generic;
namespace ReservationRestaurant.Data
{
    public class SittingType
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public List<TimeSlot> TimeSlots { get; set; }
    }
}
