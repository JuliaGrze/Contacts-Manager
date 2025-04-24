using Microsoft.Data.SqlClient;
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

            //Fluent API
            modelBuilder.Entity<Person>().Property(temp => temp.TIN)
                .HasColumnName("TaxIdentificationNumber")
                .HasColumnType("varchar(8)")
                .HasDefaultValue("ABC12345");

            //PersonName Unique
            //modelBuilder.Entity<Person>().HasIndex(temp => temp.PersonName).IsUnique();

            //HasCheckConstraint
            modelBuilder.Entity<Person>().HasCheckConstraint("CHK_TIN", "len([TaxIdentificationNumber]) = 8");

            //Table Relations - nie jest konieczne bo EF samo to zrobi
            //modelBuilder.Entity<Person>(entity =>
            //{
            //    entity.HasOne<Country>(c => c.Country)
            //    .WithMany(p => p.Persons)
            //    .HasForeignKey(p => p.CountryID);
            //});
        }

        //EF Stored Procedure - zwraca wszytskich persons
        public List<Person> sp_GetAllPersons()
        {
            return Persons.FromSqlRaw("EXECUTE [dbo].[GetAllPersons]").ToList();
        }

        //EF Stored Procedure with Paremtrs - dodanie nowego person
        public int sp_InsertPerson(Person person)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PersonID", person.PersonID),
                new SqlParameter("@PersonName", (object?)person.PersonName ?? DBNull.Value),
                new SqlParameter("@Email", (object?)person.Email ?? DBNull.Value),
                new SqlParameter("@DateOfBirth", (object?)person.DateOfBirth ?? DBNull.Value),
                new SqlParameter("@Gender", (object?)person.Gender ?? DBNull.Value),
                new SqlParameter("@CountryID", (object?)person.CountryID ?? DBNull.Value),
                new SqlParameter("@Address", (object?)person.Address ?? DBNull.Value),
                new SqlParameter("@ReceiveNewsLetters", (object?)person.ReceiveNewsLetters ?? DBNull.Value)
            };

            return Database.ExecuteSqlRaw("EXEC [dbo].[InsertPerson]@PersonID, @PersonName, @Email, @DateOfBirth, @Gender, @CountryID, @Address, @ReceiveNewsLetters", parameters);
        }

        //EF Stored Procedure with Paremtrs - Usuniecie person
        public int sp_DeletePerson(Guid personID)
        {
            SqlParameter[] parameters = new SqlParameter[] {
                new SqlParameter("@PersonID", personID)
            };

            return Database.ExecuteSqlRaw("EXEC [dbo].[DeletePerson] @PersonID", parameters);
        }
    }
}
