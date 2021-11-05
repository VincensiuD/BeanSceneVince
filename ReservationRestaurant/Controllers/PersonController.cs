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

namespace ReservationRestaurant.Controllers
{
    public class PersonController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<IdentityUser> _userManager;

        public PersonController(ApplicationDbContext context, UserManager<IdentityUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Person
        public async Task<IActionResult> Index()
        {
            return View(await _context.People.ToListAsync());
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
            return View();
        }

        // POST: Person/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,FirstName,LastName,UserId,Email,Phone")] Person person)
        {
            if (ModelState.IsValid)
            {
                _context.Add(person);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(person);
        }

        // GET: Person/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return NotFound();
            }
            return View(person);
        }

        // POST: Person/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,FirstName,LastName,UserId,Email,Phone")] Person person)
        {
            if (id != person.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(person);
                    await _context.SaveChangesAsync();
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
                return RedirectToAction(nameof(Index));
            }
            return View(person);
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
