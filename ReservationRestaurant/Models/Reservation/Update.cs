using Microsoft.AspNetCore.Mvc.Rendering;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Models.Reservation
{
    public class Update: Models.Reservation.Create
    {
        [Required]
        [Display(Name = "Reservation Id")]
        public int Id { get; set; }
        [Display(Name = "Selected Tables")]
        public List<int> SelectedTablesIds { get; set; } = new List<int>();  //the ID's of the selected Tables or int[] 
        public List<SelectListItem> Tables { get; set; } = new List<SelectListItem>();
        public List<Table> ExistingTables { get; set; } = new List<Table>();
        [Display(Name = "Reservation Status")]
        public SelectList ReservationStatuses { get; set; }
        [Display(Name = "Reservation Origin")]
        public SelectList ReservationOrigins { get; set; }
        public Sitting Sitting { get; set; }
        public DateTime ReservationStartDateTime { get; set; }
    }
}
