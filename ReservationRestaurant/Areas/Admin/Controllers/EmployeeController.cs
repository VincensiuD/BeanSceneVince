using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Areas.Admin.Controllers
{
    [Authorize(Roles = "Manager")]
    public class EmployeeController : AdminAreaBaseController
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PersonService _personService;
        public EmployeeController(ApplicationDbContext context, UserManager<IdentityUser> userManager, PersonService personService) : base(context)
        {
            _userManager = userManager;
            _personService = personService;
            var config = new MapperConfiguration(cfg => cfg.CreateMap<Models.Employee.Update, Data.Person>().ReverseMap());
            _mapper = new Mapper(config);
        }
        #region Index
        public async Task<IActionResult> Index()
        {
            var person = new Person();           
            var employeeListWithTitle = new List<Models.Employee.UpdateRole>();
            var empUsers = await _userManager.GetUsersInRoleAsync("Employee"); //get all users in Employee role 
            var people = await _context.People.ToListAsync();
            foreach (var empUser in empUsers)
            {
                person = people.Where(p => p.UserId == empUser.Id).FirstOrDefault(); //compare user Id 
                var empRoles = await _userManager.GetRolesAsync(empUser);
                var updateRoleModel = new Models.Employee.UpdateRole()
                {
                    Id = person.Id,
                    UserId = person.UserId,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Phone = person.Phone,
                    Email = person.Email,
                };
                foreach (var role in empRoles)
                { 
                    if (role == "Manager")
                    {
                        updateRoleModel.Title = "Manager";
                        break;
                    }
                    else
                    {
                        updateRoleModel.Title = "Employee";
                    }
                }
                employeeListWithTitle.Add(updateRoleModel);
            }
            return View(employeeListWithTitle);
        }
        #endregion
        #region Create
        [HttpGet]
        public IActionResult Create()
        {
            var m = new Models.Employee.Create();
            return View(m);
        }

        [HttpPost]
        public async Task<IActionResult> Create(Models.Employee.Create m)
        {
            if (ModelState.IsValid)
            {
                var user = new IdentityUser { UserName = m.Email, PhoneNumber = m.Phone, Email = m.Email };
                var result = await _userManager.CreateAsync(user, m.Password);
                if (result.Succeeded)
                {
                    await _userManager.AddToRolesAsync(user, new List<string> { "Member", "Employee" });
                    var p = new Person
                    {
                        FirstName = m.FirstName,
                        LastName = m.LastName,
                        UserId = user.Id,
                        Email = m.Email,
                        Phone = m.Phone
                    };
                    await _personService.UpsertPersonAsync(p, false);// NO NEED to allow upadte, it should be false because we're going to create new employee 
                    return RedirectToAction("Index", "Employee", new { area = "Admin" });
                }
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            //If we got this far, something failed, redisplay form
            return View(m);
        }
        #endregion
        #region Update-EmployeeInfo
        [HttpGet]
        public async Task<IActionResult> Update(int? id)
        {
            if (!id.HasValue)
            {
                return NotFound();
            }
            var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id);
            if (person == null)
            {
                return NotFound();
            }
            var m = _mapper.Map<Models.Employee.Update>(person);
            return View(m);
        }

        [HttpPost]
        public async Task<IActionResult> Update(int id, Models.Employee.Update m)
        {
            if (id != m.Id)
            {
                return NotFound();
            }
            if (ModelState.IsValid)
            {
                try
                {
                    var p = _mapper.Map<Data.Person>(m);
                    _context.Update<Person>(p);
                    await _context.SaveChangesAsync();
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception ex)
                {
                    //log the exception
                }
            }
            return View(m);
        }
        #endregion
        #region UpdateRoleToManager
        [HttpGet]
        public async Task<IActionResult> UpdateRoleToManager(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id.Value);
                if (person == null)
                {
                    return NotFound();
                }
                return View(person);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }

        [HttpPost]
        [ActionName("UpdateRoleToManager")]
        public async Task<IActionResult> UpdateRoleToManager(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.AddToRoleAsync(user, "Manager");
            return RedirectToAction(nameof(Index));
        }
        #endregion
        #region RemoveManagerRole
        [HttpGet]
        public async Task<IActionResult> RemoveManagerRole(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id.Value);
                if (person == null)
                {
                    return NotFound();
                }
                return View(person);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }      
        [HttpPost]
        [ActionName("RemoveManagerRole")]
        public async Task<IActionResult> RemoveManagerRole(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            await _userManager.RemoveFromRoleAsync(user, "Manager");
            await _userManager.AddToRoleAsync(user, "Employee");
            return RedirectToAction(nameof(Index));
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
                var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id.Value);
                if (person == null)
                {
                    return NotFound();
                }
                var m = new Models.Employee.Delete()
                {
                    Id = person.Id,
                    FirstName=person.FirstName,
                    LastName=person.LastName,
                    Phone=person.Phone,
                    Email=person.Email
                };
                return View(m);
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        
        [HttpPost]
        [ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed(int? id, Models.Employee.Delete m)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                
                var person = await _context.People.FirstOrDefaultAsync(p => p.Id == id.Value); //get the person with associated id
                var user = await _userManager.FindByIdAsync(person.UserId.ToString()); //use the person from people table to get the user 
                if (person == null)
                {
                    return NotFound();
                }

                if (m.FromTheSystem)
                {
                    var reservations = await _context.Reservations.Include(r => r.Person).Where(r => r.PersonId == person.Id)
                                                                  .Include(r => r.Sitting)
                                                                  .ThenInclude(s => s.SittingType)
                                                                  .Include(r => r.ReservationStatus)
                                                                  .Include(r => r.ReservationOrigin)
                                                                  .Include(r => r.Tables)
                                                                  .ToListAsync();
                    await _userManager.RemoveFromRolesAsync(user, new List<string> { "Employee", "Member" });
                    await _userManager.DeleteAsync(user);
                    _context.Reservations.RemoveRange(reservations);
                    _context.People.Remove(person);
                    await _context.SaveChangesAsync();
                }
                else
                {
                    await _userManager.RemoveFromRoleAsync(user, "Employee");//remove the user from the Employee List 
                }
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