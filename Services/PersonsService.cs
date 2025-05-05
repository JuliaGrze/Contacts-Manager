using CsvHelper;
using CsvHelper.Configuration;
using Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.VisualBasic;
using OfficeOpenXml;
using RepositoryContracts;
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
        private readonly IPersonsRepository _personsRepository;

        //constructor
        public PersonsService(IPersonsRepository personsRepository)
        {
            _personsRepository = personsRepository;
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
            await _personsRepository.AddPerson(person);

            //convert the Person object into PersonResponse type
            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetAllPersons()
        {
            //Select * fromn Persons
            var persons = await _personsRepository.GetAllPersons();
            return persons.Select(person => person.ToPersonResponse()).ToList();
            //return _db.sp_GetAllPersons().Select(person => ConvertPersonToPersonResponse(person)).ToList();
        }

        public async Task<PersonResponse?> GetPersonByPersonID(Guid? personID)
        {
            //check if personID is null
            if (personID == null)
                return null;

            Person person = await _personsRepository.GetPersonById(personID.Value);
            if (person == null)
                return null;

            return person.ToPersonResponse();
        }

        public async Task<List<PersonResponse>> GetFilteredPersons(string searchBy, string? searchString)
        {
            List<Person> persons = searchBy switch
            {
                nameof(PersonResponse.PersonName) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.PersonName.ToLower().Contains(searchString.ToLower())),

                nameof(PersonResponse.Email) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.Email.ToLower().Contains(searchString.ToLower())),

                nameof(PersonResponse.DateOfBirth) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.DateOfBirth.HasValue &&
                        person.DateOfBirth.Value.ToString("d").Contains(searchString)),

                nameof(PersonResponse.Gender) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.Gender.ToLower() == searchString.ToLower()),

                nameof(PersonResponse.CountryID) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.Country.CountryName.ToLower().Contains(searchString.ToLower())),

                nameof(PersonResponse.Address) =>
                    await _personsRepository.GetFilteredPersons(person =>
                        person.Address.ToLower().Contains(searchString.ToLower())),

                _ => await _personsRepository.GetAllPersons()
            };


            return persons.Select(temp => temp.ToPersonResponse()).ToList();
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
            Person? matchingPerson = await _personsRepository.GetPersonById(personUpdateRequest.PersonID);

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

            await _personsRepository.UpdatePerson(matchingPerson);

            return matchingPerson.ToPersonResponse();
        }

        public async Task<bool> DeletePerson(Guid? personID)
        {
            //check if "personID" is not null
            if(personID == null) 
                throw new ArgumentNullException(nameof(personID));

            //Get the matching "Person" object from List<Person> based on PersonID
            Person? matchingPerson = await _personsRepository.GetPersonById(personID.Value);

            //check if matching Person object is not null
            if (matchingPerson == null)
                return false;

            //Delete the matching Person object from List<person>
            await _personsRepository.DeletePersonByPersonID(personID.Value); //DELETE

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

            List<PersonResponse> people = await GetAllPersons();

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

        public async Task<MemoryStream> GetPersonsExcel()
        {
            MemoryStream memoryStream = new MemoryStream();

            List<PersonResponse> people = await GetAllPersons();

            //tworzy nowy pakiet Excela w pamięci – to jakbyś tworzył nowy plik .xlsx, ale jeszcze bez zapisania go fizycznie.
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                //dodaje nowy arkusz (ang. worksheet) o nazwie "PersonsSheet" do tego zeszytu
                ExcelWorksheet worksheet = excelPackage.Workbook.Worksheets.Add("PersonsSheet");

                //naglowki
                worksheet.Cells["A1"].Value = "Person Name";
                worksheet.Cells["B1"].Value = "Email";
                worksheet.Cells["C1"].Value = "Date Of Birth";
                worksheet.Cells["D1"].Value = "Age";
                worksheet.Cells["E1"].Value = "Gender";
                worksheet.Cells["F1"].Value = "Country";
                worksheet.Cells["G1"].Value = "Address";
                worksheet.Cells["H1"].Value = "Receive News Letters";

                using(ExcelRange headerCells = worksheet.Cells["A1:H1"])
                {
                    headerCells.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                    headerCells.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
                    headerCells.Style.Font.Bold = true;
                }

                //dane
                int row = 2;
                foreach(PersonResponse person in people)
                {
                    worksheet.Cells[row, 1].Value = person.PersonName;
                    worksheet.Cells[row, 2].Value = person.Email;
                    worksheet.Cells[row, 3].Value = person.DateOfBirth?.ToString("yyyy-MM-dd");
                    worksheet.Cells[row, 4].Value = person.Age;
                    worksheet.Cells[row, 5].Value = person.Gender;
                    worksheet.Cells[row, 6].Value = person.Country;
                    worksheet.Cells[row, 7].Value = person.Address;
                    worksheet.Cells[row, 8].Value = person.ReceiveNewsLetters;
                    row++;
                }

                worksheet.Cells[$"A1:H{row}"].AutoFitColumns(); //automatyczne dostowanie szerowkosci kolumn
                await excelPackage.SaveAsync(); // <--- async zapisywanie
            }

            memoryStream.Position = 0;
            return memoryStream;
        }
    }
}
