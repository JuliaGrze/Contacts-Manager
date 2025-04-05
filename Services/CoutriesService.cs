using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CoutriesService : ICountriesService
    {
        //private field
        private readonly List<Country> _countries;

        //constructor
        public CoutriesService()
        {
            _countries = new List<Country>();
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
            if (_countries.Where(country => country.CountryName == countryAddRequest.CountryName).Count() > 0)
                throw new ArgumentException("Given country name already exist");

            //Convert object from CountryAddRequest to Country type
            Country country = countryAddRequest.ToCountry();

            //gnerate CountryID
            country.CountryID = Guid.NewGuid();

            //Add Country object into _countries
            _countries.Add(country);

            return country.ToCountryResponse();
        }

        public List<CountryResponse> GetAllCountries()
        {         
            return _countries.Select(country => country.ToCountryResponse()).ToList();
        }

        public CountryResponse? GetCountryByCountryID(Guid? countryID)
        {
            if(countryID == null) return null;
            return _countries.FirstOrDefault(country => country.CountryID == countryID).ToCountryResponse();
        }
    }
}
