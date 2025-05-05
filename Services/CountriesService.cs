using Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using RepositoryContracts;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        //private field
        private readonly ICountriesRepository _countriesRepository;

        //constructor
        public CountriesService(ICountriesRepository countriesRepository)
        {
            _countriesRepository = countriesRepository;
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
            if (await _countriesRepository.GetCountryByCountryName(countryAddRequest.CountryName) != null) //await
                throw new ArgumentException("Given country name already exist");

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //gnerate CountryID
            country.CountryID = Guid.NewGuid();

            //Add Country object into _countries
            await _countriesRepository.AddCountry(country);        

            return country.ToCountryResponse();
        }

        public async Task<List<CountryResponse>> GetAllCountries()
        {         
            return (await _countriesRepository.GetAllCountries()).Select(country => country.ToCountryResponse()).ToList();
        }

        public async Task<CountryResponse?> GetCountryByCountryID(Guid? countryID)
        {
            if (countryID == null) return null;

            var country = await _countriesRepository.GetCountryByCountryId(countryID.Value);

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
                        if (await _countriesRepository.GetCountryByCountryName(countryName) == null)
                        {
                            Country country = new Country()
                            {
                                CountryName = countryName,
                            };

                            await _countriesRepository.AddCountry(country);

                            countriesInserted++;
                        }
                    }
                }               
            }

            return countriesInserted;
        }
    }
}
