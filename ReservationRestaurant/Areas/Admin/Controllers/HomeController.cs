using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Controllers
{
   
    public class HomeController : AdminAreaBaseController
    {
        public HomeController(ApplicationDbContext context) : base(context) { }

        public IActionResult Index()
        {
            return View();
        }
    }
}
