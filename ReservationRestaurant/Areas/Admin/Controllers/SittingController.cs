﻿using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Controllers
{
    
    public class SittingController : AdminAreaBaseController
    {
        //private Mapper _mapper;//I added this
        public SittingController(ApplicationDbContext context) : base(context)
        {
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Models.Sitting.Update, Data.Sitting>().ReverseMap());
            _mapper = new Mapper(config);
        }
        #region Index
        [Authorize(Roles ="Employee")]
        public IActionResult Index(string sortOrder)
        {
            var listOfSittings = _context.Sittings.Include(x => x.SittingType).Include(x => x.Restaurant).Include(x=>x.Reservations).ToList();
            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name" : "Name";
            ViewBag.DateSortParm = String.IsNullOrEmpty(sortOrder) ? "Date" : "Date";
            ViewBag.TypeSortParm = String.IsNullOrEmpty(sortOrder) ? "Sitting" : "Sitting";
            ViewBag.BoolSortParm = String.IsNullOrEmpty(sortOrder) ? "Bool" : "Bool";

            switch (sortOrder)
            {
                case "Name":
                    listOfSittings = listOfSittings.OrderBy(s => s.Name).ToList();
                    break;
                case "Date":
                    listOfSittings = listOfSittings.OrderBy(s => s.StartTime).ToList();
                    break;
                case "Sitting":
                    listOfSittings = listOfSittings.OrderBy(s => s.SittingTypeId).ToList();
                    break;
                case "Bool":
                    listOfSittings = listOfSittings.OrderBy(s => s.IsClosed).ToList();
                    break;
                default:
                    listOfSittings = listOfSittings.OrderBy(s => s.StartTime).ToList();
                    break;
            }
            
            return View(listOfSittings);

        }
        #endregion

        #region Create
        [Authorize(Roles = "Manager")]

        [HttpGet]
        public IActionResult Create()
        {
            var modelSittingCreate = new Models.Sitting.Create();

            return ReturnWithSittingSelectList(modelSittingCreate);
        }

       

        [HttpPost]
        public IActionResult Create(Models.Sitting.Create modelSittingCreate)
        {
            if (ModelState.IsValid)
            {
                //the code below checking if end time is earlier than start time

                DateTime startingDate = modelSittingCreate.StartTime;
                TimeSpan timeStart = TimeSpan.Parse(modelSittingCreate.Time1);
                TimeSpan timeEnd = TimeSpan.Parse(modelSittingCreate.Time2);

                if(timeEnd < timeStart)
                {
                    ViewBag.ValidationMessage =
                               $"End time should be later than start time";
                    return ReturnWithSittingSelectList(modelSittingCreate);
                }

                // the code below checking if any sitting has been previously made on that day
                var existingSitting = _context.Sittings.Where
                    (x => x.SittingTypeId == modelSittingCreate.SittingTypeId).ToList();                        


                foreach (var item in existingSitting)
                {
                    for (int i = 0; i < modelSittingCreate.Amount; i++)
                    {
                        if (item.StartTime.Date == startingDate.Date.AddDays(i))
                        {                          

                            ViewBag.ValidationMessage = 
                                $"You have previously created the same time of sitting on the" +
                                $" {item.StartTime.Date.ToShortDateString()} ";
                            return ReturnWithSittingSelectList(modelSittingCreate);
                        }
                    }

                }
                                
                try
                {
                    
                    DateTime startingDateTime = startingDate.Add(timeStart);
                    DateTime endingDateTime = startingDate.Add(timeEnd);


                    for (int i = 0; i < modelSittingCreate.Amount; i++)
                    {

                        Sitting stg = new Sitting
                        {
                            SittingTypeId = modelSittingCreate.SittingTypeId,
                            StartTime = startingDateTime.AddDays(i),
                            EndTime = endingDateTime.AddDays(i),
                            Capacity = modelSittingCreate.Capacity,
                            RestaurantId = 1,
                            Name = modelSittingCreate.Name,
                            IsClosed = modelSittingCreate.IsClosed
                        };
                        _context.Sittings.Add(stg);
                        _context.SaveChanges();
                    }
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception)
                {

                    return ReturnWithSittingSelectList(modelSittingCreate);
                }

            }
            return ReturnWithSittingSelectList(modelSittingCreate);
        }


        private IActionResult ReturnWithSittingSelectList(Models.Sitting.Create modelSittingCreate)
        {
            modelSittingCreate.SittingTypeSL
                            = new SelectList(_context.SittingTypes.ToArray(), nameof(SittingType.Id), nameof(SittingType.Name));


            return View(modelSittingCreate);
        }

        #endregion

        #region Update
        [Authorize(Roles = "Manager")]

        [HttpGet]
        public IActionResult Update(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id not found");
                }
                var sitting = _context.Sittings.Include(x => x.SittingType).FirstOrDefault(x => x.Id == id);
                if (id == null)
                {
                    return NotFound();
                }
                var sittingTypeSL = new SelectList(_context.SittingTypes.ToArray(), nameof(SittingType.Id), nameof(SittingType.Name));


                Models.Sitting.Update modelSittingUpdate = new Models.Sitting.Update()
                {
                    SittingTypeSL = sittingTypeSL,
                    SittingTypeId = sitting.SittingTypeId,
                    StartTime = sitting.StartTime.Date,
                    Time1 = sitting.StartTime.TimeOfDay.ToString(),
                    Time2 = sitting.EndTime.TimeOfDay.ToString(),
                    Capacity = sitting.Capacity,
                    RestaurantId = 1,
                    Name = sitting.Name,
                    IsClosed = sitting.IsClosed 

                };
                return View(modelSittingUpdate);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        
        [HttpPost]
        public IActionResult Update(int? id, Models.Sitting.Update modelSittingUpdate)
        {
            if (id != modelSittingUpdate.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {

                DateTime startingDate = modelSittingUpdate.StartTime;
                TimeSpan timeStart = TimeSpan.Parse(modelSittingUpdate.Time1);
                TimeSpan timeEnd = TimeSpan.Parse(modelSittingUpdate.Time2);

                if (timeEnd < timeStart)
                {
                    ViewBag.ValidationMessage =
                               $"End time should be later than start time";
                    return ReturnWithSittingSelectList(modelSittingUpdate);
                }
                
                try
                {
                    Sitting sitting = _mapper.Map<Data.Sitting>(modelSittingUpdate);
                    sitting.StartTime = modelSittingUpdate.StartTime.Add(timeStart);
                    sitting.EndTime = modelSittingUpdate.StartTime.Add(timeEnd);
                    sitting.RestaurantId = 1;

                   

                    _context.Sittings.Update(sitting);
                    _context.SaveChanges();

                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    return StatusCode(500, e.Message);
                }
            }
            modelSittingUpdate.SittingTypeSL = new SelectList(_context.SittingTypes.ToArray(), nameof(SittingType.Id), nameof(SittingType.Name));
            return View(modelSittingUpdate);
        }

        #endregion

        #region Delete
        [Authorize(Roles = "Manager")]

        [HttpGet]
        public IActionResult Delete(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id not found");
                }
                var sitting = _context.Sittings.Include(x => x.SittingType).Include(x => x.Reservations)
                                                                            .Include(s=>s.Restaurant)
                                                                            .FirstOrDefault(x => x.Id == id);
                if (id == null)
                {
                    return NotFound();
                }
                return View(sitting);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }


        [HttpPost]
        public async Task<IActionResult> Delete(int? id, Sitting sitting)
        {
            if (id != sitting.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    if (!id.HasValue)
                    {
                        return StatusCode(400, "Id not found");
                    }
                    if (id == null)
                    {
                        return NotFound();
                    }
                    //var reservations = await _context.Reservations.Include(r => r.Sitting).Where(r => r.SittingId == sitting.Id)
                    //                                     .Include(r => r.Person)
                    //                                     .Include(r => r.ReservationStatus).ToListAsync();
                    //var sittingList = await _context.Sittings.Include(s => s.Reservations).Where(s => s.Id == sitting.Id)
                    //                                        .ToListAsync();// the sitting which I want to delete including all the reservation which is belong to this sitting

                    var sittingList = await _context.Sittings.Include(s => s.Reservations).ThenInclude(s=>s.Tables)
                        .Include(s=>s.SittingType)
                        .Include(s=>s.Restaurant).FirstOrDefaultAsync(s => s.Id == sitting.Id);
                    
                    _context.Sittings.Remove(sittingList);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception e)
                {
                    return StatusCode(500, e.Message);
                    
                }

                
            }
            return RedirectToAction("Delete", new { id });
        }

        #endregion

        #region Details
        [Authorize(Roles = "Manager")]

        public IActionResult Details(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id not found");
                }
                var sitting = _context.Sittings.Include(x => x.SittingType).Include(x => x.Restaurant).Include(s=>s.Reservations).FirstOrDefault(x => x.Id == id);
                if (id == null)
                {
                    return NotFound();
                }
                return View(sitting);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        #endregion

        #region Report
        [Authorize(Roles = "Manager")]

        public async Task<IActionResult> Report(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id not found");
                }
                var sitting = await _context.Sittings.Include(x => x.SittingType).Include(x => x.Restaurant).Include(x => x.Reservations).ThenInclude(x => x.Tables).FirstOrDefaultAsync(x => x.Id == id);
                var reservations = _context.Reservations.Include(r => r.Tables).Where(r => r.SittingId == sitting.Id);
                var tables = 0;
                var phoneBookings = 0;
                var emailBookings = 0;
                var onlineBookings = 0;
                var walkInBookings = 0;

                foreach (var item in reservations)
                {
                    tables += item.Tables.Count();

                    switch (item.ReservationOriginId.ToString())
                    {
                        case "1":
                            phoneBookings++;
                            break;
                        case "2":
                            emailBookings++;
                            break;
                        case "3":
                            onlineBookings++;
                            break;
                        case "4":
                            walkInBookings++;
                            break;
                    }
                }

                var m = new Models.Sitting.Report
                {
                    Id = sitting.Id,
                    Name = sitting.Name,
                    StartTime = sitting.StartTime,
                    EndTime = sitting.EndTime,
                    Capacity = sitting.Capacity,
                    ReservationCount = sitting.Reservations.Count(),
                    pax = sitting.Pax,
                    Tables = tables,
                    SittingType = sitting.SittingType,
                    Vacanies = sitting.Vacancies,
                    NumberOfPhoneBookings = phoneBookings,
                    NumberOfEmailBookings = emailBookings,
                    NumberOfOnlineBookings = onlineBookings,
                    NumberOfWalkInBookings = walkInBookings

                };
                if (id == null)
                {
                    return NotFound();
                }
                return View(m);
            }
            catch (Exception)
            {
                return StatusCode(500);
            }
        }

        #endregion
        #region ReservationList
        [Authorize(Roles = "Manager,Employee")]

        public IActionResult ReservationList(int? id)
        {
            //Here we have to include the reservationStatus , ReservationRegion and Person which are in the ReservationList to make it visable in the view
            // Or Can we pass it as ViewwBag or ViewData
            var sitting = _context.Sittings.Include(s => s.SittingType)
                                            .Include(s => s.Reservations).ThenInclude(r=>r.ReservationStatus)
                                            .Include(s => s.Reservations).ThenInclude(r => r.ReservationOrigin)
                                            .Include(s => s.Reservations).ThenInclude(r => r.Person)
                                            .Include(s => s.Restaurant)
                                            .FirstOrDefault(s => s.Id == id);
            //var sitting = _context.Sittings.Include(x => x.Reservations)
            //                                .FirstOrDefault(x => x.Id == id);
            var reservationList = sitting.Reservations.ToList();
            return View(sitting);
        }

        #endregion
        #region DeleteAll
        [Authorize(Roles = "Manager")]

        public async Task<IActionResult> DeleteAll(int? id) //deletes all reservations in a sitting 
        {
            var sitting = await _context.Sittings.Include(s => s.SittingType)
                                           .Include(s => s.Reservations).ThenInclude(r => r.ReservationStatus)
                                           .Include(s => s.Reservations).ThenInclude(r => r.ReservationOrigin)
                                           .Include(s => s.Reservations).ThenInclude(r => r.Person)
                                           .Include(s => s.Reservations).ThenInclude(r => r.Tables)
                                           .Include(s => s.Restaurant)
                                           .FirstOrDefaultAsync(s => s.Id == id);
            //var sitting = _context.Sittings.Include(x => x.Reservations)
            //                                .FirstOrDefault(x => x.Id == id);
            var reservationList = sitting.Reservations.ToList();
            foreach (var item in reservationList)
            {
                _context.Remove(item);
            }
            _context.SaveChanges();

            return RedirectToAction("ReservationList", new { id });
        }

        #endregion
    }
}