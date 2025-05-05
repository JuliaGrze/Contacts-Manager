using Entities;
using Microsoft.EntityFrameworkCore;
using RepositoryContracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Repositories
{
    public class PersonsRepository : IPersonsRepository
    {
        private readonly ApplicationDbContext _db;
        public PersonsRepository(ApplicationDbContext applicationDbContext)
        {
            _db = applicationDbContext;
        }

        public async Task<Person> AddPerson(Person person)
        {
            _db.Persons.Add(person);
            await _db.SaveChangesAsync();

            return person;
        }

        public async Task<bool> DeletePersonByPersonID(Guid personID)
        {
            var person = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == personID);
            if (person == null) return false;

            _db.Persons.Remove(person);
            int rows = await _db.SaveChangesAsync();

            return rows > 0;
        }


        public async Task<List<Person>> GetAllPersons()
        {
            return await _db.Persons.Include("Country").ToListAsync();
        }

        public async Task<List<Person>> GetFilteredPersons(Expression<Func<Person, bool>> predicate)
        {
            return await _db.Persons
                .Include(temp => temp.Country)
                .Where(predicate)
                .ToListAsync();
        }

        public async Task<Person?> GetPersonById(Guid personID)
        {
            return await _db.Persons
                .Include(temp => temp.Country)
                .FirstOrDefaultAsync(temp => temp.PersonID == personID);
        }

        public async Task<Person> UpdatePerson(Person person)
        {
            if (person == null)
                throw new ArgumentNullException(nameof(person));

            var matchingPerson = await _db.Persons.FirstOrDefaultAsync(p => p.PersonID == person.PersonID);

            if (matchingPerson == null)
                return person;

            matchingPerson.PersonName = person.PersonName;
            matchingPerson.Email = person.Email;
            matchingPerson.DateOfBirth = person.DateOfBirth;
            matchingPerson.CountryID = person.CountryID;
            matchingPerson.Gender = person.Gender; 
            matchingPerson.Address = person.Address;
            matchingPerson.ReceiveNewsLetters = person.ReceiveNewsLetters;

            int countUpdated = await _db.SaveChangesAsync();

            return matchingPerson;
        }

    }
}
