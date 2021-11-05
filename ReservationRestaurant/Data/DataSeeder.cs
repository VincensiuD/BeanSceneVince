using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Data
{
    public class DataSeeder
    {
        //DataSeeding
        public DataSeeder(ModelBuilder mb)
        {
            mb.Entity<Restaurant>()
                .HasData(new Restaurant { Id = 1, Name = "Bean Scene", Address="69 Bean Street", PhoneNumber="04 8888 9999" });

            mb.Entity<Area>()
                .HasData(
                new Area { Id = 1, RestaurantId = 1, Name = "Main" },
                new Area { Id = 2, RestaurantId = 1, Name = "Outside" },
                new Area { Id = 3, RestaurantId = 1, Name = "Balcony" }
                );
            mb.Entity<Table>()
                .HasData(
                new Table { Id = 1, AreaId = 1, Name = "M1" },
                new Table { Id = 2, AreaId = 1, Name = "M2" },
                new Table { Id = 3, AreaId = 1, Name = "M3" },
                new Table { Id = 4, AreaId = 1, Name = "M4" },
                new Table { Id = 5, AreaId = 1, Name = "M5" },
                new Table { Id = 6, AreaId = 1, Name = "M6" },
                new Table { Id = 7, AreaId = 1, Name = "M7" },
                new Table { Id = 8, AreaId = 1, Name = "M8" },
                new Table { Id = 9, AreaId = 1, Name = "M9" },
                new Table { Id = 10, AreaId = 1, Name = "M10" },
                new Table { Id = 11, AreaId = 2, Name = "O1" },
                new Table { Id = 12, AreaId = 2, Name = "O2" },
                new Table { Id = 13, AreaId = 2, Name = "O3" },
                new Table { Id = 14, AreaId = 2, Name = "O4" },
                new Table { Id = 15, AreaId = 2, Name = "O5" },
                new Table { Id = 16, AreaId = 2, Name = "O6" },
                new Table { Id = 17, AreaId = 2, Name = "O7" },
                new Table { Id = 18, AreaId = 2, Name = "O8" },
                new Table { Id = 19, AreaId = 2, Name = "O9" },
                new Table { Id = 20, AreaId = 2, Name = "O10" },
                new Table { Id = 21, AreaId = 3, Name = "B1" },
                new Table { Id = 22, AreaId = 3, Name = "B2" },
                new Table { Id = 23, AreaId = 3, Name = "B3" },
                new Table { Id = 24, AreaId = 3, Name = "B4" },
                new Table { Id = 25, AreaId = 3, Name = "B5" },
                new Table { Id = 26, AreaId = 3, Name = "B6" },
                new Table { Id = 27, AreaId = 3, Name = "B7" },
                new Table { Id = 28, AreaId = 3, Name = "B8" },
                new Table { Id = 29, AreaId = 3, Name = "B9" },
                new Table { Id = 30, AreaId = 3, Name = "B10" }
                );

            mb.Entity<ReservationOrigin>()
                .HasData(
                    new ReservationOrigin { Id = 1, Name = "Phone" },
                    new ReservationOrigin { Id = 2, Name = "Email" },
                    new ReservationOrigin { Id = 3, Name = "Online" },
                    new ReservationOrigin { Id = 4, Name = "In Person" }
                );

            mb.Entity<ReservationStatus>()
                .HasData(
                    new ReservationStatus { Id = 1, Name = "Pending" },
                    new ReservationStatus { Id = 2, Name = "Confirmed" },
                    new ReservationStatus { Id = 3, Name = "Cancelled" },
                    new ReservationStatus { Id = 4, Name = "Seated" },
                    new ReservationStatus { Id = 5, Name = "Complete" }
                );
            mb.Entity<SittingType>()
                .HasData(
                    new SittingType { Id = 1, Name = "Breakfast" },
                    new SittingType { Id = 2, Name = "Lunch" },
                    new SittingType { Id = 3, Name = "Dinner" },
                    new SittingType { Id = 4, Name = "Other" }
                );
            
            mb.Entity<TimeSlot>()
                .HasData(
                    new TimeSlot { Id = 1, Time = "07:00 AM", SittingTypeId=1 },
                    new TimeSlot { Id = 2, Time = "07:15 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 3, Time = "07:30 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 4, Time = "07:45 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 5, Time = "08:00 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 6, Time = "08:15 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 7, Time = "08:30 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 8, Time = "08:45 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 9, Time = "09:00 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 10, Time = "09:15 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 11, Time = "09:30 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 12, Time = "09:45 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 13, Time = "10:00 AM", SittingTypeId = 1 },
                    new TimeSlot { Id = 14, Time = "12:00 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 15, Time = "12:15 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 16, Time = "12:30 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 17, Time = "12:45 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 18, Time = "1:00 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 19, Time = "1:15 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 20, Time = "1:30 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 21, Time = "1:45 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 22, Time = "2:00 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 23, Time = "2:15 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 24, Time = "2:30 PM", SittingTypeId = 2 },
                    new TimeSlot { Id = 25, Time = "5:30 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 26, Time = "5:45 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 27, Time = "6:00 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 28, Time = "6:15 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 29, Time = "6:30 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 30, Time = "6:45 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 31, Time = "7:00 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 32, Time = "7:15 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 33, Time = "7:30 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 34, Time = "7:45 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 35, Time = "8:00 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 36, Time = "8:15 PM", SittingTypeId = 3 },
                    new TimeSlot { Id = 37, Time = "8:30 PM", SittingTypeId = 3 }    
                );
            
            //mb.Entity<Sitting>()
            //    .HasData(
            //        new Sitting { Id = 1, Name= "Middle Eastern Breakfast", StartTime = new DateTime(DateTime.Now.Year, 01, 01, 09, 00,00), EndTime = new DateTime(DateTime.Now.Year, 01, 01, 12, 00, 00), Capacity = 30, RestaurantId = 1, SittingTypeId = 1, IsClosed=false },
            //        new Sitting { Id = 2, Name = "Middle Eastern Lunch", StartTime = new DateTime(DateTime.Now.Year, 01, 01, 13, 00, 00), EndTime = new DateTime(DateTime.Now.Year, 01, 01, 16, 00, 00), Capacity = 30, RestaurantId = 1, SittingTypeId = 2, IsClosed = false },
            //        new Sitting { Id = 3, Name = "Middle Eastern Dinner", StartTime = new DateTime(DateTime.Now.Year, 01, 01, 17, 00, 00), EndTime = new DateTime(DateTime.Now.Year, 01, 01, 21, 00, 00), Capacity = 30, RestaurantId = 1, SittingTypeId = 3, IsClosed = false }

            //    );
        }
    }
}

