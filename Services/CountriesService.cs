using Entities;
using ServiceContracts;
using ServiceContracts.DTO;

namespace Services
{
    public class CountriesService : ICountriesService
    {
        //private field
        private readonly List<Country> _countries;

        //constructor
        public CountriesService(bool initialize = true)
        {
            _countries = new List<Country>();
            if (initialize)
            {
                _countries.Add(new Country { CountryID = Guid.Parse("722AEA05-1CBA-46F5-BD98-95FA3A40F298"), CountryName = "USA" });
                _countries.Add(new Country { CountryID = Guid.Parse("51E77A42-2938-4120-8F03-A78283C097D1"), CountryName = "Canada" });
                _countries.Add(new Country { CountryID = Guid.Parse("39E5283C-9915-4A9A-8A29-734D4F2CB2A9"), CountryName = "UK" });
                _countries.Add(new Country { CountryID = Guid.Parse("0100D2BE-EFD9-469F-BDC2-73284D19BD6E"), CountryName = "India" });
                _countries.Add(new Country { CountryID = Guid.Parse("12E7AE0D-E4E5-4BF5-856A-5BBA1D384BBA"), CountryName = "Australia" });
            }
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
            return _countries.FirstOrDefault(country => country.CountryID == countryID)?.ToCountryResponse();
        }
    }
}
