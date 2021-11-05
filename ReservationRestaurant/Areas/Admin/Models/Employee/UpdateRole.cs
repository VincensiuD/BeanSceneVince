using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Models.Employee
{
    public class UpdateRole:Update
    {
        public string Title { get; set; }
    }
}