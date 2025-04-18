using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Nodes;
using System.Threading.Tasks;

namespace Entities
{
    public class PersonsDbContext : DbContext
    {
        //łaściwości, które mówią EF, że chcesz pracować z tabelami Countries i Persons
        public DbSet<Country> Countries { get; set; }
        public DbSet<Person> Persons { get; set; }

        //Ten konstruktor umożliwia przekazanie konfiguracji bazy danych (np. connection stringa). 
        public PersonsDbContext(DbContextOptions options) : base(options) { }

        //Określa jak klasy są mapowane na tabele oraz Pozwala na seedowanie danych (czyli dodanie danych początkowych).
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<Country>().ToTable("Countries");
            modelBuilder.Entity<Person>().ToTable("Persons");

            //Mówisz EF, że klasa Country ma mapować się na tabelę Countries, a Person na Persons.
            //Seed to Countries
            string countries = System.IO.File.ReadAllText("countries.json");
            List<Country> countryList = System.Text.Json.JsonSerializer.Deserialize<List<Country>>(countries);
            foreach (Country country in countryList)
            {
                modelBuilder.Entity<Country>().HasData(country);
            }

            //Seed to Persons
            string persons = File.ReadAllText("persons.json");
            List<Person> personsList = System.Text.Json.JsonSerializer.Deserialize<List<Person>>(persons);
            foreach (Person person in personsList)
            {
                modelBuilder.Entity<Person>().HasData(person);
            }
        }

        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }
    }
}
