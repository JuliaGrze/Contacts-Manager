using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        //private field
        private readonly PersonsDbContext _db;

        //constructor
        public CountriesService(PersonsDbContext personsDbContext)
        {
            _db = personsDbContext;
        }

        public async Task<CountryResponse> AddCountry(CountryAddRequest? countryAddRequest)
        {
            //Validation: countryAddRequest can't be null
            if(countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));         

            //Validation: countryName can't be null
            if(countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            //Validation: countryName can't be duplicate
            if (await _db.Countries.CountAsync(country => country.CountryName == countryAddRequest.CountryName) > 0) //await
                throw new ArgumentException("Given country name already exist");

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //gnerate CountryID
            country.CountryID = Guid.NewGuid();

            //Add Country object into _countries
            _db.Countries.Add(country); 
            await _db.SaveChangesAsync(); //zapisuje wszystkie zmiany wykonane w kontekście bazy danych (_db) do rzeczywistej bazy danych.

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {         
            return await _db.Countries.Select(country => country.ToCountryResponse()).ToListAsync();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            if (countryID == null) return null;

            var country = await _db.Countries.FirstOrDefaultAsync(c => c.CountryID == countryID);
            return country?.ToCountryResponse();
        }

        public async Task<int> UploadCountriesFromExcelFile(IFormFile formFile)
        {
            MemoryStream memoryStream = new MemoryStream();
            //Zamienia plik z formularza (formFile) na MemoryStream, żeby móc go odczytać w pamięci.
            await formFile.CopyToAsync(memoryStream);

            int countriesInserted = 0;

            // Tworzy obiekt ExcelPackage (czyli reprezentację pliku Excel) na podstawie wczytanego strumienia.
            using (ExcelPackage excelPackage = new ExcelPackage(memoryStream))
            {
                // Wybiera arkusz o nazwie Countries.
                ExcelWorksheet excelWorksheet = excelPackage.Workbook.Worksheets["Countries"];
                if (excelWorksheet == null)
                    throw new Exception("Worksheet 'Countries' not found in Excel file.");

                //Liczy, ile jest wierszy w arkuszu (czyli ile krajów próbujesz dodać).
                int rowCount = excelWorksheet.Dimension.Rows;
                

                for(int i = 2; i <= rowCount; i++)
                {
                    //odczytywanie i wiersza
                    string? cellValue = Convert.ToString(excelWorksheet.Cells[i, 1].Value);

                    if (!string.IsNullOrEmpty(cellValue)) {
                        string? countryName = cellValue;

                        // Sprawdza, czy dany kraj już istnieje w bazie.
                        if (_db.Countries.Where(temp => temp.CountryName == countryName).Count() == 0)
                        {
                            Country country = new Country()
                            {
                                CountryName = countryName,
                            };

                            _db.Countries.Add(country);
                            await _db.SaveChangesAsync();
                            countriesInserted++;
                        }
                    }
                }               
            }

            return countriesInserted;
        }
    }
}
