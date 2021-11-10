using Microsoft.EntityFrameworkCore;
using ReservationRestaurant.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ReservationRestaurant.Service
{
    public class PersonService
    {
        private readonly ApplicationDbContext _context;
        public PersonService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Person> UpsertPersonAsync (Person data, bool update)
        {
            var person = await _context.People.FirstOrDefaultAsync(p => p.Email == data.Email);
            if (person != null)
            {
                person.FirstName = data.FirstName;
                person.LastName = data.LastName;
                person.Email = data.Email;
                person.Phone = data.Phone;
                if (update)
                {
                    person.UserId = data.UserId;
                }

            }
            if (person == null)
            {
                person = new Person
                {
                    FirstName = data.FirstName,
                    LastName = data.LastName,
                    Email= data.Email,
                    Phone=data.Phone,
                    UserId=data.UserId      
                };
                _context.People.Add(person);
            }
        
            await _context.SaveChangesAsync();
            return person;
        }
    }
}
