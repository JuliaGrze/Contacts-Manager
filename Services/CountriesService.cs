using Entities;
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

        public CountryResponse AddCountry(CountryAddRequest? countryAddRequest)
        {
            //Validation: countryAddRequest can't be null
            if(countryAddRequest == null)
                throw new ArgumentNullException(nameof(countryAddRequest));         

            //Validation: countryName can't be null
            if(countryAddRequest.CountryName == null)
                throw new ArgumentException(nameof(countryAddRequest.CountryName));

            //Validation: countryName can't be duplicate
            if (_db.Countries.Where(country => country.CountryName == countryAddRequest.CountryName).Count() > 0)
                throw new ArgumentException("Given country name already exist");

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //gnerate CountryID
            country.CountryID = Guid.NewGuid();

            //Add Country object into _countries
            _db.Countries.Add(country);
            _db.SaveChanges(); //zapisuje wszystkie zmiany wykonane w kontekście bazy danych (_db) do rzeczywistej bazy danych.

            return country.ToCountryResponse();
        }

        public List<CountryResponse> GetAllCountries()
        {         
            return _db.Countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryByCountryID(Guid? countryID)
        {
            if(countryID == null) return null;
            return _db.Countries.FirstOrDefault(country => country.CountryID == countryID)?.ToCountryResponse();
        }
    }
}
