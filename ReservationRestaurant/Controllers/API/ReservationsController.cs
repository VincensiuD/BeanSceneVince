using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Controllers.API
{
    [Route("api/[controller]")]
    [ApiController]
    public class ReservationsController : ControllerBase
    {

        private readonly ApplicationDbContext _context;

        public ReservationsController(ApplicationDbContext context)
        {
            _context = context;
        }


        [HttpGet, Route("")]
        public async Task<IActionResult> Get()
        {
           var sittingList = await  _context.Reservations.Include(x => x.ReservationOrigin).
                //Include(x => x.Sitting).ThenInclude(x => x.SittingType).
                Include(x => x.Tables).ToListAsync();

            return Ok(sittingList);
        }


        [HttpGet, Route("{id}")]
        public async Task<IActionResult> GetListOfSittings(int id)
        {
            var sittingList = await _context.Sittings.Include(x => x.SittingType)
                .Where(x => x.SittingTypeId == id).ToListAsync();

            if (sittingList != null)
            {
                return Ok(sittingList); 
            }

            return NotFound();
        }

        [HttpGet, Route("{id}/xyz")]
        public IActionResult GetById2(int id)
        {

            return Ok(new { id, scer = "234"});
        }
    }
}
