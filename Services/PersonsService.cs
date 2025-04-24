using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        //private field
        private readonly PersonsDbContext _db;
        private readonly ICountriesService _countriesService;

        //constructor
        public PersonsService(PersonsDbContext personsDbContext, ICountriesService countriesService)
        {
            _db = personsDbContext;
            _countriesService = countriesService;

        }     

        public async Task<PersonResponse> AddPerson(PersonAddRequest? personAddRequest)
        {
            //check if PersonAddRequest is not null
            if (personAddRequest == null)
            {
                throw new ArgumentNullException(nameof(personAddRequest));
            }

            //Model validation
            ValidationHelper.ModelValidation(personAddRequest);

            //convert personAddRequest into Person type
            Person person = personAddRequest.ToPerson();

            //generate PersonID
            person.PersonID = Guid.NewGuid();

            //add person object to persons list
            _db.Persons.Add(person);
            await _db.SaveChangesAsync(); //executte insert into database
            //_db.sp_InsertPerson(person);

            //convert the Person object into PersonResponse type
            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            //Select * fromn Persons
            var persons = await _db.Persons.Include(p => p.Country).ToListAsync();
            return persons.Select(person => person.ToPersonResponse()).ToList();
            //return _db.sp_GetAllPersons().Select(person => ConvertPersonToPersonResponse(person)).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            //check if personID is null
            if (personID == null)
                return null;

            Person person = await _db.Persons.Include(p => p.Country).FirstOrDefaultAsync(person => person.PersonID == personID);
            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = await GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrWhiteSpace(searchBy) || string.IsNullOrWhiteSpace(searchString))
                return matchingPersons;

            switch (searchBy)
            {
                case nameof(PersonResponse.PersonName):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrWhiteSpace(person.PersonName) &&
                         person.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(PersonResponse.Email):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Email) &&
                        person.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(PersonResponse.DateOfBirth):
                    if (DateTime.TryParse(searchString, out DateTime searchDate))
                    {
                        matchingPersons = allPersons
                            .Where(person => person.DateOfBirth?.Date == searchDate.Date)
                            .ToList();
                    }
                    break;

                case nameof(PersonResponse.Gender):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Gender) &&
                        person.Gender.Equals(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(PersonResponse.CountryID):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Country) &&
                        person.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(PersonResponse.Address):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Address) &&
                        person.Address.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                default:
                    matchingPersons = allPersons;
                    break;
            }

            return matchingPersons;
        }

        public async Task<List<PersonResponse>> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderEnum sortOrder)
        {
            if (sortBy == null || sortOrder == null)
                return allPersons;

            List<PersonResponse> sortedPersons = (sortBy, sortOrder)
                switch
            {
                //PersonName
                (nameof(PersonResponse.PersonName), SortOrderEnum.ASC) =>
                    allPersons.OrderBy(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.PersonName), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.PersonName, StringComparer.OrdinalIgnoreCase).ToList(),

                // Email
                (nameof(PersonResponse.Email), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Email), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.Email, StringComparer.OrdinalIgnoreCase).ToList(),

                //DateofBirt
                (nameof(PersonResponse.DateOfBirth), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.DateOfBirth).ToList(),
                (nameof(PersonResponse.DateOfBirth), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.DateOfBirth).ToList(),

                //Gender
                (nameof(PersonResponse.Gender), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Gender), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.Gender, StringComparer.OrdinalIgnoreCase).ToList(),

                //Country
                (nameof(PersonResponse.Country), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Country), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.Country, StringComparer.OrdinalIgnoreCase).ToList(),

                //Address
                (nameof(PersonResponse.Address), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),
                (nameof(PersonResponse.Address), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.Address, StringComparer.OrdinalIgnoreCase).ToList(),

                //ReceiveNewsLetters
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.ReceiveNewsLetters).ToList(),
                (nameof(PersonResponse.ReceiveNewsLetters), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.ReceiveNewsLetters).ToList(),

                //Age
                (nameof(PersonResponse.Age), SortOrderEnum.ASC) =>
                   allPersons.OrderBy(temp => temp.Age).ToList(),
                (nameof(PersonResponse.Age), SortOrderEnum.DESC) =>
                    allPersons.OrderByDescending(temp => temp.Age).ToList()
            };

            return sortedPersons;
        }

        public async Task<PersonResponse> UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            //check if personUpdateRequest is not null
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            //validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //Get the match Person object fromList<person> based on PersonID to update
            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == personUpdateRequest.PersonID);

            //check if matching Person is null
            if (matchingPerson == null)
                throw new ArgumentException("Given person id it doesn't exist");

            //Update all details from PersonUpdateRequest object to PersonObject       
            matchingPerson.PersonName = personUpdateRequest.PersonName;
            matchingPerson.Email = personUpdateRequest.Email;
            matchingPerson.DateOfBirth = personUpdateRequest.DateOfBirth;
            matchingPerson.Gender = personUpdateRequest.Gender.ToString();
            matchingPerson.CountryID = personUpdateRequest.CountryID;
            matchingPerson.Address = personUpdateRequest.Address;
            matchingPerson.ReceiveNewsLetters = personUpdateRequest.ReceiveNewsLetters;

            await _db.SaveChangesAsync(); //exexcute Update in Database

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            //check if "personID" is not null
            if(personID == null) 
                throw new ArgumentNullException(nameof(personID));

            //Get the matching "Person" object from List<Person> based on PersonID
            Person? matchingPerson = await _db.Persons.FirstOrDefaultAsync(temp => temp.PersonID == personID);

            //check if matching Person object is not null
            if(matchingPerson == null)
                return false;

            //Delete the matching Person object from List<person>
            _db.Persons.Remove(_db.Persons.First(temp => temp.PersonID == personID));
            await _db.SaveChangesAsync(); //DELETE
            //_db.sp_DeletePerson(personID.Value);

            return true;

        }

        //MemoryStream to klasa w .NET, która umożliwia zapisywanie danych do pamięci operacyjnej (RAM) zamiast do pliku na dysku.
        //Chcesz wygenerować dane dynamicznie (np. CSV, PDF, obraz) i wysłać je użytkownikowi bez zapisywania ich na dysku.
        public async Task<MemoryStream> GetPersonCSV()
        {
            ////tworzysz pusty strumień danych w pamięci RAM. To będzie tymczasowy „plik”, który nie istnieje na dysku.
            MemoryStream memoryStream = new MemoryStream(); 

            // Tworzysz obiekt StreamWriter, który umożliwia zapis tekstu (czyli znaków) do MemoryStream. Inaczej mówiąc – dzięki temu możesz pisać do strumienia jak do pliku tekstowego.
            StreamWriter writer = new StreamWriter(memoryStream);

            CsvConfiguration csvConfiguration = new CsvConfiguration(CultureInfo.InvariantCulture);

            CsvWriter csvWriter = new CsvWriter(writer, csvConfiguration);

            //PersonName, Emial, DateOfBirth, Age, Gender, Country, Address, ReceiveNewsLetters
            csvWriter.WriteField(nameof(PersonResponse.PersonName));
            csvWriter.WriteField(nameof(PersonResponse.Email));
            csvWriter.WriteField(nameof(PersonResponse.DateOfBirth));
            csvWriter.WriteField(nameof(PersonResponse.Age));
            csvWriter.WriteField(nameof(PersonResponse.Gender));
            csvWriter.WriteField(nameof(PersonResponse.Country));
            csvWriter.WriteField(nameof(PersonResponse.Address));
            csvWriter.WriteField(nameof(PersonResponse.ReceiveNewsLetters));
            csvWriter.NextRecord();

            List<PersonResponse> people = _db.Persons.Include(p => p.Country).Select(p => p.ToPersonResponse()).ToList();

            foreach (PersonResponse person in people)
            {
                csvWriter.WriteField(person.PersonName);
                csvWriter.WriteField(person.Email);
                if (person.DateOfBirth.HasValue)
                    csvWriter.WriteField(person.DateOfBirth?.ToString("d"));
                else
                    csvWriter.WriteField("");
                csvWriter.WriteField(person.Age);
                csvWriter.WriteField(person.Gender);
                csvWriter.WriteField(person.Country);
                csvWriter.WriteField(person.Address);
                csvWriter.WriteField(person.ReceiveNewsLetters);
                csvWriter.NextRecord();
            }

            // Resetuje pozycję, by móc czytać od początku
            memoryStream.Position = 0;

            return memoryStream;    // Zwracasz gotowy CSV jako strumień
        }
    }
}
