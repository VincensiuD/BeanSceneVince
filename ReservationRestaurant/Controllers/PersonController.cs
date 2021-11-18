using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using ReservationRestaurant.Service;

namespace ReservationRestaurant.Controllers
{
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly PersonService _personService;
        public PersonController(ApplicationDbContext context, UserManager<IdentityUser> userManager, PersonService personService)
        {
            _context = context;
            _personService = personService;
            _userManager = userManager;
        }

        // GET: Person
        public IActionResult Index()
        {
            return View();
        }

        public IActionResult IndexList(string searchString, string option, string sortOrder, string? role)
        {
            var users = GetListofUsers(role).Result.ToList();

            ViewData["CurrentFilter"] = searchString;
            ViewData["CurrentRole"] = role;

            ViewBag.NameSortParm = String.IsNullOrEmpty(sortOrder) ? "Name" : "Name";
            ViewBag.RoleSortParm = String.IsNullOrEmpty(sortOrder) ? "Role" : "Role";
            switch (sortOrder)
            {
                case "Name":
                    users = users.OrderBy(p => p.LastName).ToList();
                    break;
                case "Role":
                    users = users.OrderBy(p => p.Title).ToList();
                    break;
                default:
                    users = users.OrderBy(p => p.Id).ToList();
                    break;
            }
            if (!String.IsNullOrEmpty(searchString))
            {
                if (option == "Name")
                {
                    users = users.Where(m => m.LastName.Contains(searchString, StringComparison.OrdinalIgnoreCase)).ToList();
                }
                else if (option == "Role")
                {
                    users = users.Where(m => m.Title.Contains(searchString)).ToList();
                }
                else
                {
                    return View(users);
                }
            }
            return View(users);
        }

        private async Task<List<Models.Person.Details>> GetListofUsers(string? role)//string role
        {
            IList<IdentityUser> users = null;
            var person = new Person();
            var peopleListWithTitle = new List<Models.Person.Details>();
            var managers = new List<Models.Person.Details>();
            var employees = new List<Models.Person.Details>();
            var members = new List<Models.Person.Details>();
            var nonMembers = new List<Models.Person.Details>();
            var people = await _context.People.ToListAsync();
            foreach (var p in people)
            {
                if (p.UserId == null)
                {
                    var modelPersonDetails = new Models.Person.Details()
                    {
                        Id = p.Id,
                        UserId = p.UserId,
                        FirstName = p.FirstName,
                        LastName = p.LastName,
                        Email = p.Email,
                        Phone = p.Phone,
                        Title = "User- Not Registered"
                    };
                    peopleListWithTitle.Add(modelPersonDetails);
                    nonMembers.Add(modelPersonDetails);
                }
            }
            users = await _userManager.GetUsersInRoleAsync("Member");// get all users== 
            foreach (var user in users)
            {
                person = people.Where(p => p.UserId == user.Id).FirstOrDefault();
                var userRoles = await _userManager.GetRolesAsync(user);
                var modelPersonDetails = new Models.Person.Details()
                {
                    Id = person.Id,
                    UserId = person.UserId,
                    FirstName = person.FirstName,
                    LastName = person.LastName,
                    Email = person.Email,
                    Phone = person.Phone,
                };
                for (int i = 0; i < userRoles.Count; i++)
                {
                    if (await _userManager.IsInRoleAsync(user, "Manager"))
                    {
                        modelPersonDetails.Title = "Manager";
                        peopleListWithTitle.Add(modelPersonDetails);
                        managers.Add(modelPersonDetails);
                        break;
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Employee"))
                    {
                        modelPersonDetails.Title = "Employee";
                        peopleListWithTitle.Add(modelPersonDetails);
                        employees.Add(modelPersonDetails);
                        break;
                    }
                    else if (await _userManager.IsInRoleAsync(user, "Member"))
                    {
                        modelPersonDetails.Title = "Member";
                        peopleListWithTitle.Add(modelPersonDetails);
                        members.Add(modelPersonDetails);
                    }
                }
            }
            switch (role)
            {
                case "Manager":
                    return managers;
                case "Employee":
                    return employees;
                case "Member":
                    return members;
                case "NotMember":
                    return nonMembers;
                default:
                    return peopleListWithTitle;
            }
        }


        // GET: Person/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            try
            {
                if (!id.HasValue)
                {
                    return StatusCode(400, "Id required");
                }
                var person = await _context.People.Include(p=> p.Reservations).FirstOrDefaultAsync(p => p.Id == id.Value);
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
        [Authorize(Roles = "Member")]
        public async Task<IActionResult> MyDetails()
        {
            try
            {
                var user = await _userManager.FindByNameAsync(User.Identity.Name);
                var person = await _context.People.FirstOrDefaultAsync(p => p.UserId == user.Id);
                if (person == null)
                {
                    return NotFound();
                }
                return RedirectToAction(nameof(Details),new { person.Id});
            }
            catch (Exception ex)
            {
                return StatusCode(500);
            }
        }
        // GET: Person/Create
        public IActionResult Create()
        {
            var model = new Models.Person.Create();
            return View(model);
        }

        // POST: Person/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Models.Person.Create model)
        {
            if (ModelState.IsValid)
            {
                Person person = new Person()
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Email = model.Email,
                    Phone = model.Phone
                };


                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People.FindAsync(id);

            Models.Person.Create model = new Models.Person.Create()
            {

                FirstName = person.FirstName,
                LastName = person.LastName,
                Email = person.Email,
                Phone = person.Phone,
                UserId = person.UserId
            };

            if (person == null)
            {
                return NotFound();
            }
            return View(model);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Models.Person.Create model)
        {

            if (id != model.Id)
            {
                return NotFound();
            }
            Person person2 = new Person();
            bool isMember = true;

            if (ModelState.IsValid)
            {

                if(model.UserId == null)
                {
                    isMember = false;
                }

                Person person = new Person();

                person.FirstName = model.FirstName;
                person.LastName = model.LastName;
                person.Email = model.Email;
                person.Phone = model.Phone;
                person.UserId = model.UserId;
                    try
                    {

                      person2 = await _personService.UpsertPersonAsync(person, isMember);   
                    
                    

                    }
                    catch (DbUpdateConcurrencyException)
                    {
                        if (!PersonExists(person.Id))
                        {
                            return NotFound();
                        }
                        else
                        {
                            throw;
                        }
                    }
                
                return RedirectToAction(nameof(Details),new {person2.Id});
            }

            return View(model);
        }

        // GET: Person/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People
                .FirstOrDefaultAsync(m => m.Id == id);
            if (person == null)
            {
                return NotFound();
            }

            return View(person);
        }

        // POST: Person/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var person = await _context.People.FindAsync(id);
            var reservations = await _context.Reservations.Include(r => r.Person).Where(r => r.PersonId == person.Id)
                                                          .Include(r => r.Sitting)
                                                          .ThenInclude(s => s.SittingType)
                                                          .Include(r => r.ReservationStatus)
                                                          .Include(r => r.ReservationOrigin)
                                                          .Include(r => r.Tables)
                                                          .ToListAsync();
             if (person.UserId != null)
            {
                var user = await _userManager.FindByIdAsync(person.UserId);
                await _userManager.RemoveFromRolesAsync(user,new List<string>{ "Employee", "Member"});
                await _userManager.DeleteAsync(user);
            }
            _context.Reservations.RemoveRange(reservations);                                            
            _context.People.Remove(person);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool PersonExists(int id)
        {
            return _context.People.Any(e => e.Id == id);
        }
    }
}
