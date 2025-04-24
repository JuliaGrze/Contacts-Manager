using Entities;
using Microsoft.EntityFrameworkCore;
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

    }
}
