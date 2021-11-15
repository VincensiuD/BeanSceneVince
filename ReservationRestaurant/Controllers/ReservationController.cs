using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace ReservationRestaurant.Controllers
{
    public class ReservationController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly PersonService _personService;
        private readonly UserManager<IdentityUser> _userManager;
        private IMapper _mapper;
        private iEmailService _iEmailService;
        public ReservationController(ApplicationDbContext context, PersonService personService, UserManager<IdentityUser> userManager, iEmailService iEmailService)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Models.Reservation.Update, Data.Reservation>().ReverseMap().ForAllOtherMembers(x => x.Ignore()));
            _mapper = new Mapper(config);
            _context = context;
            _personService = personService;
            _userManager = userManager;
            _iEmailService = iEmailService;
        }
        #region Index
        [Authorize(Roles = "Employee")]
        public IActionResult Index()
        {
            return View();
        }
        [Authorize(Roles = "Employee")]
        public IActionResult IndexList(string searchString, string option, string sortOrder, string status)
        {
            var reservations = GetReservationsByStatus(status).Result;
            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentStatus"] = status;
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name" : "Name";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "Date";
            ViewBag.TypeSortParm = String.IsNullOrEmpty(sortOrder) ? "Sitting" : "Sitting";
            switch (sortOrder)
            {
                case "Name":
                    reservations = reservations.OrderBy(s => s.Person.LastName).ToList();
                    break;
                case "Date":
                    reservations = reservations.OrderBy(s => s.StartTime).ToList();
                    break;
                case "Sitting":
                    reservations = reservations.OrderBy(s => s.Sitting.SittingTypeId).ToList();
                    break;
                default:
                    reservations = reservations.OrderBy(s => s.Id).ToList();
                    break;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                if (option == "Name")
                {
                    reservations = reservations.Where(s => s.Person.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                else if (option == "Email")
                {
                    reservations = reservations.Where(s => s.Person.Email.Contains(searchString)).ToList();
                }

                else if (option == "ReservationId")
                {
                    reservations = reservations.Where(s => s.Id == int.Parse(searchString)).ToList();
                }
                else
                {
                    return View(reservations);
                }
            }
            return View(reservations);
        }
        private async Task<List<Reservation>> GetReservationsByStatus(string status)
        {
            var allReservation = await _context.Reservations.Include(r => r.Person)
                                                      .Include(r => r.Sitting)
                                                      .ThenInclude(s => s.SittingType)
                                                      .Include(r => r.ReservationStatus)
                                                      .Include(r => r.ReservationOrigin)
                                                      .Include(r => r.Tables)
                                                      .OrderBy(r => r.Id).ToListAsync();
            switch (status)
            {
                case "Pending":
                    return allReservation.Where(r => r.ReservationStatus.Name == "Pending").ToList();
                case "Confirmed":
                    return allReservation.Where(r => r.ReservationStatus.Name == "Confirmed").ToList();
                case "Cancelled":
                    return allReservation.Where(r => r.ReservationStatus.Name == "Cancelled").ToList();
                case "Seated":
                    return allReservation.Where(r => r.ReservationStatus.Name == "Seated").ToList();
                case "Complete":
                    return allReservation.Where(r => r.ReservationStatus.Name == "Complete").ToList();
                default:
                    return allReservation.ToList();
            }
        }
        #endregion

        #region PreCreate
        [HttpGet]
        public async Task<IActionResult> PreCreate(int? sittingId)
        {
            var sittingList = await _context.Sittings.Include(x => x.SittingType).ToListAsync();
            string maximumBookingDate = JQueryDateSetting(sittingList);

            Models.Reservation.PreCreate preCreate = new Models.Reservation.PreCreate()
            {

                AmountOfDaysForCalendar = maximumBookingDate
            };

            if (sittingId == null)
            {
                var sittingType = await _context.SittingTypes.ToListAsync();
            }
            else if (sittingId != null)
            {
                var sitting = await _context.Sittings.FirstOrDefaultAsync(x => x.Id == sittingId);

                preCreate.StartTime = sitting.StartTime.ToShortDateString();
                preCreate.SittingTypeId = sitting.SittingTypeId;
                preCreate.AmountOfDaysForCalendar = maximumBookingDate;
            }
            ViewBagOpenSitting(sittingList);
            return InsertSelectList(preCreate);
        }


        [HttpPost]
        public async Task<IActionResult> PreCreate(Models.Reservation.PreCreate preCreate)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    DateTime dateSelected = DateTime.Parse(preCreate.StartTime);

                    var allSitting = await _context.Sittings.Include(x => x.SittingType).Include(x => x.Reservations).ToListAsync();  //first get a list of all the available sittings

                    if (preCreate.Guests < 1)
                    {
                        preCreate.Message = $"The number of guest has to be bigger than 1";
                        ViewBagOpenSitting(allSitting);
                        return InsertSelectList(preCreate);
                    }

                    //this returns particular sitting that the customer had chosen, based on the date and sittingType (bfast,lunch, etc)
                    var selectedSitting = allSitting.FirstOrDefault(x => x.StartTime.ToShortDateString() == dateSelected.ToShortDateString() && x.SittingTypeId == preCreate.SittingTypeId);
                    if (selectedSitting == null)
                    {
                        preCreate.Message = $"The selected day ({dateSelected.ToShortDateString()}) doesn't have that particular sittings. " +
                            "Please contact the restaurant to check if we can accommodate your request";
                        ViewBagOpenSitting(allSitting);
                        return InsertSelectList(preCreate);
                    }
                    if (selectedSitting.IsClosed == true || selectedSitting.Vacancies < preCreate.Guests)
                    {
                        preCreate.Message = $"The sitting ({selectedSitting.SittingType.Name}) for selected day is full. " +
                            "Please contact the restaurant to check if we can accommodate your request";
                        ViewBagOpenSitting(allSitting);
                        return InsertSelectList(preCreate);
                    }
                    var create = new Models.Reservation.Create()
                    {
                        StartTime = preCreate.StartTime,
                        Guests = preCreate.Guests,
                        SittingTypeId = preCreate.SittingTypeId,
                        SittingId = selectedSitting.Id,
                        SittingType = selectedSitting.SittingType,
                        TimeSlot = preCreate.TimeSlot
                    };

                    return RedirectToAction(nameof(Create), create);
                }

                catch (Exception)
                {

                    var sittingList2 = await _context.Sittings.Include(x => x.SittingType).ToListAsync();
                    string maximumBookingDate2 = JQueryDateSetting(sittingList2);
                    preCreate.AmountOfDaysForCalendar = maximumBookingDate2;
                    preCreate.Message = $"The booking cannot be made, please ensure all the field has been completed correctly" +
                        " and the booking date format has to be dd-mm-yyyy";
                    ViewBagOpenSitting(sittingList2);

                    return InsertSelectList(preCreate);
                }
            }
            var sittingList = await _context.Sittings.Include(x => x.SittingType).ToListAsync();
            string maximumBookingDate = JQueryDateSetting(sittingList);
            preCreate.AmountOfDaysForCalendar = maximumBookingDate;

            ViewBagOpenSitting(sittingList);

            return InsertSelectList(preCreate);
        }

        private void ViewBagOpenSitting(List<Sitting> sittingList)
        {
            ViewBag.BreakfastList = GenerateSittingAvailability(sittingList, 1).ToArray();
            ViewBag.LunchList = GenerateSittingAvailability(sittingList, 2).ToArray();
            ViewBag.DinnerList = GenerateSittingAvailability(sittingList, 3).ToArray();
        }

        private static List<string> GenerateSittingAvailability(List<Sitting> sittingList, int sittingCode)
        {

            var openingList = sittingList.Where(x => x.SittingType.Id == sittingCode
                && x.StartTime > DateTime.Today && x.IsClosed == false);

            List<string> availableDates = new List<string>();
            foreach (var item in openingList)
            {
                string day = item.StartTime.Day.ToString("00");
                string month = item.StartTime.Month.ToString("00");
                string year = item.StartTime.Year.ToString();

                string completeDate = day + "-" + month + "-" + year;
                availableDates.Add(completeDate);
            }
            return availableDates;
        }

        private static string JQueryDateSetting(List<Sitting> sittingList)
        {
            var today = DateTime.Now;
            var maxDate = sittingList.Select(x => x.StartTime).Max(); //the latest day in the sittingList
            int dateDifference = (int)(maxDate - today).TotalDays - 1; // how many days between today and last day of available sitting
            // minus two because they way DateTime count days includes the starting day (between today and tomo = 2 days)
            string maximumBookingDate = "+" + (dateDifference.ToString()) + "d"; // to get format +2d or +3d or +4d that I need to use for datepicker jquery

            string[] allDatesString = new string[dateDifference + 1];
            //Sitting[] closedArray = sittingList.Where(x => x.IsClosed == true).ToArray();

            for (int i = 0; i < dateDifference + 1; i++)
            {
                allDatesString[i] = today.AddDays(i).ToShortDateString();
            }
            return maximumBookingDate;
        }

        private IActionResult InsertSelectList(Models.Reservation.PreCreate preCreate)
        {
            preCreate.SittingTypeSL = new SelectList(_context.SittingTypes.ToArray(), nameof(SittingType.Id), nameof(SittingType.Name));
            return View(preCreate);
        }

        #endregion

        #region Create
        [HttpGet]
        public async Task<IActionResult> Create(Models.Reservation.Create mo)
        {

            try
            {
                Sitting sitting = await _context.Sittings.Include(s=> s.SittingType)
                    .FirstOrDefaultAsync(s => s.Id == mo.SittingId);

                if (sitting == null || mo.Guests < 1 || mo.StartTime == null)
                {
                    return RedirectToAction(nameof(PreCreate));
                }

                SelectList timeSlotSL = CreateTimeSlotList(sitting);

                var m = new Models.Reservation.Create // here we create reservation model instance
                {
                    Guests = mo.Guests,
                    StartTime = mo.StartTime,
                    SittingTypeId = mo.SittingTypeId,
                    SittingId = mo.SittingId,
                    TimeSL = timeSlotSL,
                    SittingType = sitting.SittingType
                };
                if (User.Identity.IsAuthenticated && !(User.IsInRole("Manager") || User.IsInRole("Employee")))   // if the user has logged in
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == user.Id);
                    if (person != null)
                    {
                        m.PersonId = person.Id;// here we add rest of reservation model instance property to pass it to the view 
                        m.Email = person.Email;
                        m.FirstName = person.FirstName;
                        m.LastName = person.LastName;
                        m.PhoneNumber = person.Phone;
                    }
                    
                }
                    List<ReservationOrigin> bookingOrigin = _context.ReservationOrigins.ToList();
                    ViewBag.ReservationOriginId = new SelectList(bookingOrigin, "Id", "Name");
                return View(m);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ActionName("Create")]
        public async Task<IActionResult> CreateForm(Models.Reservation.Create m)
        {
            Reservation reservation = null;
            var person = await _context.People.FirstOrDefaultAsync(p => p.Email == m.Email);
            if (ModelState.IsValid)
            {
                if (!User.Identity.IsAuthenticated)// new user or returned user but both are not member
                {
                    person = new Person
                    {
                        Email = m.Email,
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        Phone = m.PhoneNumber
                    };
                    person = await _personService.UpsertPersonAsync(person, false);
                }
                // else if(User.Identity.IsAuthenticated)// true if Member, Employee, Admin log in
                else
                {
                    var user = await _userManager.FindByNameAsync(User.Identity.Name);
                    if (person is null)//that means the person is new user and we haven't his info in DB--> so Admin or employee try to reserve for new user 
                    {
                        person = new Person
                        {
                            Email = m.Email,
                            FirstName = m.FirstName,
                            LastName = m.LastName,
                            Phone = m.PhoneNumber
                        };
                        person = await _personService.UpsertPersonAsync(person, false);
                    }
                    else
                    {
                        if (person.UserId != user.Id)// true if the employee or admin try to reserve for members or for returned users 
                        {
                            person = new Person
                            {
                                Email = m.Email,
                                FirstName = m.FirstName,
                                LastName = m.LastName,
                                Phone = m.PhoneNumber,
                                UserId = person.UserId
                            };
                            person = await _personService.UpsertPersonAsync(person, true);
                        }
                    }

                }
                string selectedDate = m.StartTime; //selectedDate as astring
                string selectedTime = m.TimeSlot;//get the time portion of the timeslot - still a string at this stage
                string combinedDateTime = selectedDate + " " + selectedTime;// concat the date and time
                DateTime finalDateTime = DateTime.Parse(combinedDateTime); // convert it to DateTime object

                var selectedSitting = await _context.Sittings.FirstOrDefaultAsync(s => m.SittingId == s.Id);

                DateTime endTime = finalDateTime.AddMinutes(m.Duration);
                if (endTime > selectedSitting.EndTime)
                {
                    ViewBag.Validation = $"Our shop is closing before {selectedSitting.EndTime.ToLongDateString()}" +
                        "Please book earlier";
                    m.TimeSL = CreateTimeSlotList(selectedSitting);
                    List<ReservationOrigin> bookingOrigin1 = _context.ReservationOrigins.ToList();
                    ViewBag.ReservationOriginId = new SelectList(bookingOrigin1, "Id", "Name");
                    return View(m);
                }

                //create new reservation and assign the person id
                reservation = new Reservation
                {
                    PersonId = person.Id,
                    Guests = m.Guests,
                    StartTime = finalDateTime,
                    Duration = m.Duration,
                    ReservationStatusId = m.ReservationStatusId,
                    ReservationOriginId = m.ReservationOriginId,
                    SittingId = m.SittingId,
                    SpecialRequirement = m.SpecialRequirement
                };
                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                //await _iEmailService.SendEmailAsync(m.Email, "Booking Confirmation", "<h1>Booking Confirmed</h1><p>Your Booking has been recieved</p>");
                
                return RedirectToAction(nameof(Details), new { reservation.Id });
            }
            if (!ModelState.IsValid)
            {
                var myerrors = ModelState.SelectMany(x => x.Value.Errors.Select(z => z.Exception));
                // Breakpoint, Log or examine the list with Exceptions.
                var errors = ModelState
                        .Where(x => x.Value.Errors.Count > 0)
                        .Select(x => new { x.Key, x.Value.Errors })
                        .ToArray();
                Sitting sitting = _context.Sittings.Include(s => s.SittingType).FirstOrDefault(s => s.Id == m.SittingId);
                m.TimeSL = CreateTimeSlotList(sitting);
                m.SittingType = sitting.SittingType;
            }
            List<ReservationOrigin> bookingOrigin = _context.ReservationOrigins.ToList();
            ViewBag.ReservationOriginId = new SelectList(bookingOrigin, "Id", "Name");
            return View(m);
        }

        //private static SelectList CreateTimeSlotList(Sitting sitting)
        //{
        //    DateTime startingTimeSlot = sitting.StartTime;
        //    DateTime endingTimeSlot = sitting.EndTime.AddMinutes(-45);

        //    double timeDifference = (endingTimeSlot - startingTimeSlot).TotalHours;
        //    int amountForLoop = (int)(timeDifference * 4); //times 4 because there are 4timeslots in 1hr
        //    List<(int, DateTime)> timeSlotList = new List<(int, DateTime)>();

        //    for (int i = 0; i < amountForLoop; i++)
        //    {
        //        int minutesToAdd = 15 * i;
        //        DateTime timeSlot = startingTimeSlot.AddMinutes(minutesToAdd);
        //        (int, DateTime) timeSlotTuple = ((i + 1), timeSlot);
        //        timeSlotList.Add(timeSlotTuple);
        //    }

        //    var timeSlotSelectList = timeSlotList
        //          .Select(i => new
        //          {
        //              Id = i.Item1.ToString(),
        //              timeSelection = i.Item2.ToShortTimeString(),
        //          }).ToList();


        //    SelectList timeSlotSL = new SelectList(timeSlotSelectList, "Id", "timeSelection");
        //    return timeSlotSL;
        //}

        private static SelectList CreateTimeSlotList(Sitting sitting)
        {
            DateTime startingTimeSlot = sitting.StartTime;
            DateTime endingTimeSlot = sitting.EndTime.AddMinutes(-45);

            double timeDifference = (endingTimeSlot - startingTimeSlot).TotalHours;
            int amountForLoop = (int)(timeDifference * 4); //times 4 because there are 4timeslots in 1hr
                                                           // List<(int, DateTime)> timeSlotList = new List<(int, DateTime)>();
            List<string> timeSlotList = new List<string>();

            for (int i = 0; i < amountForLoop; i++)
            {
                int minutesToAdd = 15 * i;
                DateTime DateTimeTimeSlot = startingTimeSlot.AddMinutes(minutesToAdd);
                // (int, DateTime) timeSlotTuple = ((i + 1), timeSlot);

                string stringTimeSlot = DateTimeTimeSlot.ToShortTimeString();

                timeSlotList.Add(stringTimeSlot);

            }

            SelectList timeSlotSL = new SelectList(timeSlotList, "timeSelection");
            return timeSlotSL;
        }

        #endregion

        #region Details
        public async Task<ActionResult> Details(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var reservation = await _context.Reservations.Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .FirstOrDefaultAsync(r => r.Id == id.Value);
                if (reservation == null)
                {
                    return NotFound();
                }
                return View(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        #endregion

        #region ReservationHistory
        [Authorize(Roles = "Member")]
        public async Task<ActionResult> History(int? id)
        {
            List<Reservation> reservations = null;
            if (id.HasValue)//I want to view the reservation list for someone from the PersonDetails view
            {
                var person = await _context.People.Include(p => p.Reservations).FirstOrDefaultAsync(p => p.Id == id.Value);
                if (person == null)
                {
                    return NotFound();
                }
                reservations = await _context.Reservations.Include(r => r.Person).Where(r => r.PersonId == person.Id)
                                                            .Include(r => r.Sitting)
                                                            .ThenInclude(s => s.SittingType)
                                                            .Include(r => r.ReservationStatus)
                                                            .Include(r => r.ReservationOrigin)
                                                            .Include(r => r.Tables)
                                                            .ToListAsync();
            }
            else // I want to check my reservation list (who is register and using this service) 
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == user.Id);
                reservations = await _context.Reservations.Include(r => r.Person).Where(r => r.PersonId == person.Id)
                                                            .Include(r => r.Sitting)
                                                            .ThenInclude(s => s.SittingType)
                                                            .Include(r => r.ReservationStatus)
                                                            .Include(r => r.ReservationOrigin)
                                                            .Include(r => r.Tables)
                                                            .ToListAsync();
            }
            return View(reservations);
        }

        #endregion

        #region Update
        [Authorize(Roles = "Member")]
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }
            var reservation = await _context.Reservations.Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType).ThenInclude(s => s.TimeSlots)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .FirstOrDefaultAsync(r => r.Id == id.Value);
            if (reservation == null)
            {
                return NotFound();
            }
            //var startDate = reservation.StartTime.ToString("dd/mm/yyyy");// to get the reservation date only as string
            var startDate = reservation.StartTime.ToShortDateString();// to get the reservation Time only as string
            //var selectedTimeDB = await _context.TimeSlots.FirstOrDefaultAsync(t => t.Time == startTime);
            var m = new Models.Reservation.Update // here we  create update reservation model instance
            {
                Id = reservation.Id,
                PersonId = reservation.PersonId,
                FirstName = reservation.Person.FirstName,
                LastName = reservation.Person.LastName,
                Email = reservation.Person.Email,
                PhoneNumber = reservation.Person.Phone,
                StartTime = startDate,
                //TimeSlot = selectedTimeDB,
                Guests = reservation.Guests,
                SpecialRequirement = reservation.SpecialRequirement,
                Duration = reservation.Duration,
                ReservationStatusId = reservation.ReservationStatusId,
                ReservationOriginId = reservation.ReservationOriginId,
                SittingId = reservation.SittingId,
                Sitting = reservation.Sitting,
                ExistingTables = reservation.Tables,
                ReservationStartDateTime = reservation.StartTime
            };
            var allTables = await _context.Tables.ToListAsync();
            TableMangements(m, allTables);
            return InsertSelectListsForUpdate(m);
        }

        [Authorize(Roles = "Member")]
        [HttpPost]
        public async Task<IActionResult> Update(int id, Models.Reservation.Update m)
        {
            if (id != m.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var reservation = await _context.Reservations.Include(r => r.Person)
                                                        .Include(r => r.Sitting)
                                                        .Include(r => r.ReservationStatus)
                                                        .Include(r => r.ReservationOrigin)
                                                        .Include(r => r.Tables)
                                                        .FirstOrDefaultAsync(r => r.Id == id);
                    if (reservation.Tables.Count != 0 && m.SelectedTablesIds.Count != 0)
                    {
                        reservation.Tables.Clear();
                        _context.Reservations.Update(reservation);
                        await _context.SaveChangesAsync();
                    }
                    if (m.SelectedTablesIds.Count != 0)
                    {
                        List<int> result = m.SelectedTablesIds;//here you could get the selected Tables Id's
                        List<Table> selectedTables = new List<Table>();
                        foreach (var tId in result)
                        {
                            var tableSelcted = await _context.Tables.FirstOrDefaultAsync(t => t.Id == tId);
                            selectedTables.Add(tableSelcted);
                        }
                        reservation.Tables.AddRange(selectedTables);
                    }
                    reservation.Id = m.Id;
                    reservation.PersonId = m.PersonId;
                    reservation.Guests = m.Guests;
                    reservation.StartTime = m.ReservationStartDateTime;
                    reservation.Duration = m.Duration;
                    reservation.ReservationStatusId = m.ReservationStatusId;
                    reservation.ReservationOriginId = m.ReservationOriginId;
                    reservation.SittingId = m.SittingId;
                    reservation.SpecialRequirement = m.SpecialRequirement;
                    _context.Reservations.Update(reservation);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Details), new { reservation.Id });
                }
                catch (Exception ex)
                {
                    return NotFound(ex);
                }
            }
            if (!ModelState.IsValid)
            {
                var errors = ModelState
                  .Where(x => x.Value.Errors.Count > 0)
                  .Select(x => new { x.Key, x.Value.Errors })
                  .ToArray();
            }
            var allTables = await _context.Tables.ToListAsync();
            TableMangements(m, allTables);

            m.Sitting = await _context.Sittings.Include(s => s.SittingType).FirstOrDefaultAsync(s => s.Id == m.SittingId);

            return InsertSelectListsForUpdate(m);
        }

        private static void TableMangements(Models.Reservation.Update m, List<Table> allTables)
        {
            foreach (var table in allTables)
            {
                SelectListItem selectListItem = new SelectListItem()
                {
                    Text = table.Name,
                    Value = table.Id.ToString(),
                };
                m.Tables.Add(selectListItem);
            }
            foreach (var tId in m.ExistingTables)
            {
                m.SelectedTablesIds.Add(tId.Id);
            }
        }
        private IActionResult InsertSelectListsForUpdate(Models.Reservation.Update m)
        {
            m.ReservationStatuses = new SelectList(_context.ReservationStatuses.ToArray(), nameof(ReservationStatus.Id), nameof(ReservationStatus.Name), m.ReservationStatusId);
            m.ReservationOrigins = new SelectList(_context.ReservationOrigins.ToArray(), nameof(ReservationOrigin.Id), nameof(ReservationOrigin.Name), m.ReservationOriginId);
            return View(m);
        }


        #endregion

        #region Delete
        [HttpGet]
        public async Task<IActionResult> Delete(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var reservation = await _context.Reservations.Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .FirstOrDefaultAsync(r => r.Id == id.Value);
                if (reservation == null)
                {
                    return NotFound();
                }
                return View(reservation);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var reservation = await _context.Reservations.Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .FirstOrDefaultAsync(r => r.Id == id.Value);
                if (reservation == null)
                {
                    return NotFound();
                }
                _context.Reservations.Remove(reservation);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                //log the exp
            }
            return RedirectToAction("Delete", new { id });
        }
        #endregion
    }
}
