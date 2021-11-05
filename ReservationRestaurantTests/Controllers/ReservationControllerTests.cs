using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ReservationRestaurant.Controllers;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ReservationRestaurant.Controllers.Tests
{
    [TestClass()]
    public class ReservationControllerTests
    {
        private readonly ApplicationDbContext _context;
        private readonly PersonService _personService;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ReservationController _controller;
        private readonly iEmailService _iEmailService;
        public ReservationControllerTests()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseSqlServer("Server=localhost;Database=ReservationRestaurant01;Trusted_Connection=True;MultipleActiveResultSets=true").Options;
            _context = new ApplicationDbContext(options);
            _personService = new PersonService(_context);
            _controller = new ReservationController(_context, _personService, _userManager,_iEmailService);
        }

        [TestMethod()]
        public void DeleteGetTestNotNull()// testing if Delete returns the view or not
        {
            //A-    Arange (obj instance, setup test data)
            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();
            //A-    ACt (call method to test)
            var actualResult = _controller.Delete(reservations[0].Id);
            //A-    Assert (check the return value)
            Assert.IsNotNull(actualResult);
        }
        [TestMethod()]
        public void DeleteGetModelTestNotNull()// testing if the returned Model of DeleteGet Action if not null
        {
            //A-    Arange (obj instance, setup test data)
            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();
            //A-    ACt (call method to test)
            ViewResult actualViewResult = (ViewResult)_controller.Delete(reservations[0].Id).Result;
            //A-    Assert (check the return value)
            Assert.IsNotNull(actualViewResult.Model);
        }
        [TestMethod()]
        public void Test_TypeofModel_DeleteGet()// testing if the type of returned Model from DeleteGet Action is instance of Reservation
        {
            //A-    Arange (obj instance, setup test data)
            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();
            //A-    ACt (call method to test)
            var actualViewResult = (ViewResult)_controller.Delete(reservations[0].Id).Result;
            //A-    Assert (check the return value)
            Assert.IsInstanceOfType(actualViewResult.Model, typeof(Reservation));
        }
        [TestMethod()]
        public void DeletePostTestNotNull()// testing if DeleteConfirmed returns the view or not
        {
            //A-    Arange (obj instance, setup test data)

            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();

            //A-    ACt (call method to test)
            var actualResult = _controller.DeleteConfirmed(reservations[0].Id);// if we put this-->> var actualResult = _controller.DeleteConfirmed(reservations[0].Id).Result; will implement the DeleteConfirmed Action and will Delete the reservation from the DB
                                                                                //while if we didn't add Result will test the view 
            //A-    Assert (check the return value)
            Assert.IsNotNull(actualResult);
        }
        [TestMethod()]
        public void DeleteConfirmedTestInDatabase()// Test if the deleted reservation is gone from the Reservation Entity in the DB after implement DeletConfirmed Action
        {
            //A-    Arange (obj instance, setup test data)
            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();

            //A-    ACt (call method to test)
            var result = _controller.DeleteConfirmed(reservations[0].Id);
           result.Wait();
            bool actualResult = _context.Reservations.Any(r=>r.Id== reservations[0].Id);// it should be false if the delete action is succesfully completed
            //A-    Assert (check the return value)
            Assert.IsFalse(actualResult);
        }
        [TestMethod()]
        public void Delete_all_AssociatedTablesTestFromDataBase()//Test if the all the Associated Tables are deleted when we delete the reservation from DB 
        {
            //A-    Arange (obj instance, setup test data)
            var reservations = _context.Set<Reservation>().Include(r => r.Person)
                                                              .Include(r => r.Sitting)
                                                              .ThenInclude(s => s.SittingType)
                                                              .Include(r => r.ReservationStatus)
                                                              .Include(r => r.ReservationOrigin)
                                                              .Include(r => r.Tables)
                                                              .OrderBy(r => r.Id).ToList();
            var tablesBeforeDelete = _context.Set<Table>().Include(t=>t.Reservations)
                                                .Where(t => t.Reservations.Contains(reservations[0])).ToList();
            //A-    ACt (call method to test)
            var result = _controller.DeleteConfirmed(reservations[0].Id);
            result.Wait();
            var tablesAfterDelete = _context.Set<Table>().Include(t => t.Reservations)
                                                .Where(t => t.Reservations.Contains(reservations[0])).ToList();
            //A-    Assert (check the return value)
            Assert.AreNotEqual(tablesBeforeDelete.Count, tablesAfterDelete.Count);
        }

        [TestMethod()]
        public void TestNumberofReservationsAfterDelete()//Test Number of Reservations in DB are decrement by one After DeleteConfirmed
        {
            //A-    Arange (obj instance, setup test data)
            var reservationsBeforeDelete = _context.Set<Reservation>().OrderBy(r => r.Id).ToList();

            //A-    ACt (call method to test)
            var result = _controller.DeleteConfirmed(reservationsBeforeDelete[0].Id);
            result.Wait();
            var reservationsAfterDelete = _context.Set<Reservation>().OrderBy(r => r.Id).ToList();

            //A-    Assert (check the return value)
            Assert.AreEqual((reservationsBeforeDelete.Count)-1, reservationsAfterDelete.Count);
        }
    }
}