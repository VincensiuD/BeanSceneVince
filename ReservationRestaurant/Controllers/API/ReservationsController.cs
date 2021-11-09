﻿using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Controllers.Api
{
    [Route("api/[controller]")]//[Route("api/v1/reservations")]// we can write the name of the controller directly
    [ApiController]
    public class ReservationsController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly PersonService _personService;
        private readonly UserManager<IdentityUser> _userManager;
        private IMapper _mapper;
        private iEmailService _iEmailService;
        public ReservationsController(ApplicationDbContext context, PersonService personService, UserManager<IdentityUser> userManager, iEmailService iEmailService)//
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Models.Reservation.Update, Data.Reservation>().ReverseMap().ForAllOtherMembers(x => x.Ignore()));
            _mapper = new Mapper(config);
            _context = context;
            _personService = personService;
            _userManager = userManager;
            _iEmailService = iEmailService;
        }
        [HttpGet, Route("")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Reservation))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Get()
        {
            var reservations = await _context.Reservations.ToListAsync();
            if (reservations != null)
            {
                return Ok(reservations);
            }
            return NotFound();
        }

        [HttpGet, Route("{id}")]// the name here in the Route should be the same as it in the Action Param
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Reservation))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetDetailsById(int id)
        {
            var reservation = await _context.Reservations.Include(r => r.Person)
                                                       .Include(r => r.Sitting)
                                                       .Include(r => r.ReservationStatus)
                                                       .Include(r => r.ReservationOrigin)
                                                       .Include(r => r.Tables).FirstOrDefaultAsync(r => r.Id == id);

           
            if (reservation != null)
            {
                return Ok(reservation);
            }
            return NotFound();
        }

        [HttpGet, Route("{reservationid}/tables")]// the name here in the Route should be the same as it in the Action Param
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(Table))]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetTablesReservation(int reservationid)
        {
            var reservation = await _context.Reservations.Include(r=>r.Tables).FirstOrDefaultAsync(r => r.Id == reservationid);
            var reservationTables = new List<Table>();
            if (reservation != null)
            {
                if (reservation.Tables.Count > 0)
                {
                    foreach (var table in reservation.Tables)
                    {
                        reservationTables.Add(table);
                    }
                    return Ok(reservationTables);
                }
                else
                {
                    return Ok(reservation);
                }
            }
            return NotFound();
        }

    }
}
