using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
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
            var reservations = await _context.Reservations.Include(r => r.Person)
                                                       .Include(r => r.Sitting)
                                                       .Include(r => r.ReservationStatus)
                                                       .Include(r => r.ReservationOrigin)
                                                       .Include(r => r.Tables).ToListAsync();
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

        [HttpGet, Route("{reservationid}/tables")]
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
        /// <summary>
        /// This method retrieves a particular sitting based on its SittingTypeId and StartTime
        /// This will be used to create timeslot in PreCreate page
        /// </summary>
        /// <param name="sitId">SittingTypeId of the sitting</param>
        /// <param name="dateOnly">StartTime of the sitting</param>
        /// <returns></returns>
        [HttpGet, Route("Sitting/{sitId}/{dateOnly}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSittingBySittingTypeIdAndDate(int sitId, string dateOnly)
        {
            try
            {

                DateTime date = DateTime.Parse(dateOnly);

                var sitting = _context.Sittings.FirstOrDefault(r => r.SittingTypeId == sitId && r.StartTime.Date == date);

                if (sitting != null)
                {
                    string s1 = sitting.StartTime.ToShortTimeString();
                    string s2 = sitting.EndTime.ToShortTimeString();

                    var s3 = new { start = s1, end = s2 };

                    return Ok(sitting);
                }
                return NotFound();
            }
            catch (Exception)
            {

                return NotFound();
            }
        }


        /// <summary>
        /// This method allows other app to retrieve a sitting based on its SittingId
        /// </summary>
        /// <param name="id">The id of the sitting</param>
        /// <returns>One sitting object</returns>
        [HttpGet, Route("Sitting/{id}")]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSittingById(int id)
        {
            try
            {
                var sitting = _context.Sittings.Include(x=>x.SittingType).FirstOrDefault(x => x.Id == id);

              
                return Ok(sitting);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }

        /// <summary>
        /// This method allows other app to retrieve lists of sittings based on its SittingTypeId
        /// </summary>
        /// <param name="typeId">1,2,3 or 4</param>
        /// <returns>List of sittings based on its SittingTypeId (e.g. breakfast or lunch)</returns>
        [HttpGet, Route("Sittings/{typeId}")]

        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public IActionResult GetSittingBySittingTypeId(int typeId)
        {
            try
            {
                var sitting = _context.Sittings.Include(x=>x.Reservations)
                    .Where(x=>x.SittingTypeId==typeId).ToList();


                return Ok(sitting);
            }
            catch (Exception)
            {

                return NotFound();
            }
        }
        /// <summary>
        /// ReservationDTO2 represent a model object for reservation that is made from REACT app
        /// </summary>
        public class ReservationDTO2
        {   
            [Required]
            public string Email { get; set; }
            [Range(1, int.MaxValue, ErrorMessage = "The field {0} must be at least {1}.")]
            public int Guest { get; set; }
            public int SittingId { get; set; }
            public string TimeSlot { get; set; }
            [Required]
            public string FirstName { get; set; }
            [Required]
            public string LastName { get; set; }
            [Required]
            public string Phone { get; set; }

        }

        /// <summary>
        /// Allowing other apps to post a reservation to the website
        /// </summary>
        /// <param name="dTO">dTO represents an object with reservation details frp, ReservationDTO2 class</param>
        /// <returns></returns>
        [HttpPost, Route("Create")] 
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> CreateReservation(ReservationDTO2 dTO)
        {
            try
            {
                Person person = _context.People.FirstOrDefault(x => x.Email == dTO.Email);

                Sitting sitting = _context.Sittings.FirstOrDefault(x => x.Id == dTO.SittingId);

                String date = sitting.StartTime.ToShortDateString();
                String timeslot = dTO.TimeSlot;
                String combinedTime = date + " " + timeslot;

                DateTime date2 = DateTime.Parse(combinedTime);

                if (person == null)
                {
                    person = new Person();
                    person.FirstName = dTO.FirstName;
                    person.LastName = dTO.LastName;
                    person.Email = dTO.Email;
                    person.Phone = dTO.Phone;
                }

                var reservation = new Reservation
                {
                    Person = person,
                    Guests = dTO.Guest,
                    SittingId = dTO.SittingId,
                    ReservationOriginId = 5,
                    ReservationStatusId = 1,
                    StartTime = date2,
                    Duration = 60
                };
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();
                return CreatedAtAction(nameof(Created), reservation);
            }
            catch (Exception)
            {

                return  StatusCode(400, "Error");
            }
        }

    }
}
