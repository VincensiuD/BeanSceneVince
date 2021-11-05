using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Controllers
{
    [Area("Admin"), Authorize(Roles = "Manager,Employee")]
    public abstract class AdminAreaBaseController : Controller
    {
        protected readonly ApplicationDbContext _context;
        protected Mapper _mapper;

        public AdminAreaBaseController(ApplicationDbContext context)
        {
            _context = context;
        }
    }
}
