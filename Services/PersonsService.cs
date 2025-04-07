using Entities;
using Microsoft.VisualBasic;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services
{
    public class PersonsService : IPersonsService
    {
        //private field
        private readonly List<Person> _persons;
        private readonly ICountriesService _countriesService;

        //constructor
        public PersonsService(bool initialize = true)
        {
            _persons = new List<Person>();
            _countriesService = new CountriesService();

            if (initialize)
            {
                _persons.AddRange(
                    new Person() { 
                        PersonID = Guid.Parse("15AC8137-1B42-48A6-84C7-8CBEBF3028D0"),
                        PersonName = "Jackson",
                        Email = "jtschierasche0@microsoft.com",
                        DateOfBirth = DateTime.Parse("21/10/1991"),
                        Gender = GenderOptions.Male.ToString(),
                        Address = "31505 Heath Way",
                        ReceiveNewsLetters = false,
                        CountryID = Guid.Parse("722AEA05-1CBA-46F5-BD98-95FA3A40F298")

                    },
                    new Person() { 
                        PersonID = Guid.Parse("840E0E14-5B0A-4EEE-818A-AE8322BBB27A"),
                        PersonName = "Winfield",
                        Email = "wrowles1@woothemes.com",
                        DateOfBirth = DateTime.Parse("03/05/1990"),
                        Gender = GenderOptions.Male.ToString(),
                        Address = "25617 Canary Way",
                        ReceiveNewsLetters = false,
                        CountryID = Guid.Parse("51E77A42-2938-4120-8F03-A78283C097D1")
                    },
                    new Person() { 
                        PersonID = Guid.Parse("81811502-C218-4CE4-AB38-2323323215C7"),
                        PersonName = "Mallory",
                        Email = "mkeniwell2@e-recht24.de",
                        DateOfBirth = DateTime.Parse("12/09/1991"),
                        Gender = GenderOptions.Female.ToString(),
                        Address = "14 Kipling Road",
                        ReceiveNewsLetters = true,
                        CountryID = Guid.Parse("722AEA05-1CBA-46F5-BD98-95FA3A40F298")
                    },
                    new Person() { 
                        PersonID = Guid.Parse("899E6A55-DAB1-45C5-94F6-10C8AF4F7AC1"),
                        PersonName = "Van",
                        Email = "vgillbee3@tripod.com",
                        DateOfBirth = DateTime.Parse("15/03/1996"),
                        Gender = GenderOptions.Female.ToString(),
                        Address = "8 Clemons Drive",
                        ReceiveNewsLetters = false,
                        CountryID = Guid.Parse("12E7AE0D-E4E5-4BF5-856A-5BBA1D384BBA")
                    },
                    new Person() { 
                        PersonID = Guid.Parse("4237C350-B583-47E6-944E-14108FB6ADAF"),
                        PersonName = "Ronalda",
                        Email = "rbellworthy4@msn.com",
                        DateOfBirth = DateTime.Parse("17/03/1993"),
                        Gender = GenderOptions.Female.ToString(),
                        Address = "0591 Darwin Alley",
                        ReceiveNewsLetters = false,
                        CountryID = Guid.Parse("0100D2BE-EFD9-469F-BDC2-73284D19BD6E")
                    },
                    new Person()
                    {
                        PersonID = Guid.Parse("D7844028-ED37-442C-B49C-787794BD7702"),
                        PersonName = "Michel",
                        Email = "mwilliamson5@ycombinator.com",
                        DateOfBirth = DateTime.Parse("28/08/1992"),
                        Gender = GenderOptions.Female.ToString(),
                        Address = "8 West Parkway",
                        ReceiveNewsLetters = true,
                        CountryID = Guid.Parse("51E77A42-2938-4120-8F03-A78283C097D1")
                    },
                    new Person() 
                    { 
                        PersonID = Guid.Parse("8082ED0C-396D-4162-AD1D-29A13F929824"), 
                        PersonName = "Aguste", 
                        Email = "aleddy0@booking.com", 
                        DateOfBirth = DateTime.Parse("1993-01-02"), 
                        Gender = "Male", 
                        Address = "0858 Novick Terrace", 
                        ReceiveNewsLetters = false, 
                        CountryID = Guid.Parse("722AEA05-1CBA-46F5-BD98-95FA3A40F298") 
                    },
                    new Person() 
                    { 
                        PersonID = Guid.Parse("06D15BAD-52F4-498E-B478-ACAD847ABFAA"), 
                        PersonName = "Jasmina", 
                        Email = "jsyddie1@miibeian.gov.cn", 
                        DateOfBirth = DateTime.Parse("1991-06-24"), 
                        Gender = "Female",
                        Address = "0742 Fieldstone Lane",
                        ReceiveNewsLetters = true, 
                        CountryID = Guid.Parse("51E77A42-2938-4120-8F03-A78283C097D1F") 
                    },
                    new Person() 
                    { 
                        PersonID = Guid.Parse("D3EA677A-0F5B-41EA-8FEF-EA2FC41900FD"), 
                        PersonName = "Kendall", Email = "khaquard2@arstechnica.com", 
                        DateOfBirth = DateTime.Parse("1993-08-13"), 
                        Gender = "Male", 
                        Address = "7050 Pawling Alley", 
                        ReceiveNewsLetters = false, 
                        CountryID = Guid.Parse("39E5283C-9915-4A9A-8A29-734D4F2CB2A9") 
                    },
                    new Person() 
                    { PersonID = Guid.Parse("89452EDB-BF8C-4283-9BA4-8259FD4A7A76"), 
                        PersonName = "Kilian", Email = "kaizikowitz3@joomla.org", 
                        DateOfBirth = DateTime.Parse("1991-06-17"), 
                        Gender = "Male",
                        Address = "233 Buhler Junction", 
                        ReceiveNewsLetters = true, 
                        CountryID = Guid.Parse("0100D2BE-EFD9-469F-BDC2-73284D19BD6E") 
                    }
                    );

            }
        }


        private PersonResponse ConvertPersonToPersonResponse(Person person)
        {
            PersonResponse personResponse = person.ToPersonResponse();
            personResponse.Country = _countriesService.GetCountryByCountryID(person.CountryID)?.CountryName;
            return personResponse;
        }

        public PersonResponse AddPerson(PersonAddRequest? personAddRequest)
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
            _persons.Add(person);

            //convert the Person object into PersonResponse type
            return ConvertPersonToPersonResponse(person);
        }

        public List<PersonResponse> GetAllPersons()
        {
            return _persons.Select(person => person.ToPersonResponse()).ToList();
        }

        public PersonResponse? GetPersonByPersonID(Guid? personID)
        {
            //check if personID is null
            if (personID == null)
                return null;

            return _persons.FirstOrDefault(person => person.PersonID == personID)?.ToPersonResponse();
        }

        public List<PersonResponse> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<PersonResponse> allPersons = GetAllPersons();
            List<PersonResponse> matchingPersons = allPersons;

            if (string.IsNullOrWhiteSpace(searchBy) || string.IsNullOrWhiteSpace(searchString))
                return matchingPersons;

            switch (searchBy)
            {
                case nameof(Person.PersonName):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrWhiteSpace(person.PersonName) &&
                         person.PersonName.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(Person.Email):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Email) &&
                        person.Email.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(Person.DateOfBirth):
                    if (DateTime.TryParse(searchString, out DateTime searchDate))
                    {
                        matchingPersons = allPersons
                            .Where(person => person.DateOfBirth?.Date == searchDate.Date)
                            .ToList();
                    }
                    break;

                case nameof(Person.Gender):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Gender) &&
                        person.Gender.StartsWith(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(Person.CountryID):
                    matchingPersons = allPersons
                        .Where(person => !string.IsNullOrEmpty(person.Country) &&
                        person.Country.Contains(searchString, StringComparison.OrdinalIgnoreCase))
                        .ToList();
                    break;

                case nameof(Person.Address):
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

        public List<PersonResponse> GetSortedPersons(List<PersonResponse> allPersons, string sortBy, SortOrderEnum sortOrder)
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

        public PersonResponse UpdatePerson(PersonUpdateRequest? personUpdateRequest)
        {
            //check if personUpdateRequest is not null
            if (personUpdateRequest == null)
                throw new ArgumentNullException(nameof(personUpdateRequest));

            //validation
            ValidationHelper.ModelValidation(personUpdateRequest);

            //Get the match Person object fromList<person> based on PersonID to update
            Person? matchingPerson = _persons.FirstOrDefault(temp => temp.PersonID == personUpdateRequest.PersonID);

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

            return matchingPerson.ToPersonResponse();
        }

        public bool DeletePerson(Guid? personID)
        {
            //check if "personID" is not null
            if(personID == null) 
                throw new ArgumentNullException(nameof(personID));

            //Get the matching "Person" object from List<Person> based on PersonID
            Person? matchingPerson = _persons.FirstOrDefault(temp => temp.PersonID == personID);

            //check if matching Person object is not null
            if(matchingPerson == null)
                return false;

            //Delete the matching Person object from List<person>
            _persons.RemoveAll(temp => temp.PersonID == personID);

            return true;

        }
    }
}
