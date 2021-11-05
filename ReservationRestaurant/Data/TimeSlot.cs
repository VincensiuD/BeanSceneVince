
namespace ReservationRestaurant.Data
{
    public class TimeSlot
    {
        public int Id { get; set; }
        public string Time { get; set; }
        public int SittingTypeId { get; set; }
        public SittingType SittingType { get; set; }
    }
}