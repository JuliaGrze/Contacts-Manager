using Entities;
using Microsoft.EntityFrameworkCore;
using ServiceContracts;
using ServiceContracts.DTO;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;


namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;

        public CountriesServiceTest()
        {
            _countriesService = new CountriesService(new PersonsDbContext(new DbContextOptionsBuilder<PersonsDbContext>().Options));
        }

        #region AddCountry

        //When CountryAddRequest is null, it should ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });

        }

        //When CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            //Arrange
            CountryAddRequest? request = new CountryAddRequest()
            { CountryName = null };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request);
            });
        }

        //When CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName()
        {
            //Arrange
            CountryAddRequest? request1 = new CountryAddRequest() { CountryName = "Poland" };
            CountryAddRequest? request2 = new CountryAddRequest() { CountryName = "Poland" };

            //Assert
            await Assert.ThrowsAsync<ArgumentException>(async () =>
            {
                //Act
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2);
            });
        }

        //When you supply proper CountryName, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest request = new CountryAddRequest() { CountryName = "Japan" };

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);
            List<CountryResponse> countries_from_GetAllCountries = await _countriesService.GetAllCountries();

            //Assert
            Assert.True(response.CountryID != Guid.Empty);
            Assert.Contains(response, countries_from_GetAllCountries);
        }

        #endregion


        #region GetAllCountries

        //The list of countries should be empty by default (before adding any countries)
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            //Acts
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            //Assert
            Assert.Empty(actual_country_response_list);
        }

        // It checks whether all countries in the request list are present in the response after being added.
        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            List<CountryAddRequest> country_request_list = new List<CountryAddRequest>()
            {
                new CountryAddRequest() { CountryName = "Poland" },
                new CountryAddRequest() { CountryName = "USA" },
                new CountryAddRequest() { CountryName = "UK" }
            };

            //Act
            List<CountryResponse> countries_list_from_add_country = new List<CountryResponse>();
            foreach (CountryAddRequest country_request in country_request_list)
            {
                countries_list_from_add_country.Add(await _countriesService.AddCountry(country_request));
            }
            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //read each element from countries_list_from_add_country
            foreach (CountryResponse expected_country in countries_list_from_add_country)
            {
                //Assert
                Assert.Contains(expected_country, actualCountryResponseList);
            }
            
        }
        #endregion

        #region GetCountryByCountryID

        [Fact]
        //If we supply null as CountryID, it should return null as CountryResult
        public async Task GetCountryByCountryID_NullCountryID()
        {
            //Arrange
            Guid? countryID = null;

            //Act
            CountryResponse country_resposne_from_get_method = await _countriesService.GetCountryByCountryID(countryID);

            //Assert
            Assert.Null(country_resposne_from_get_method);
        }

        [Fact]
        //If we supply a valid countryID, it should return the matching country details as CountryResult object
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            //Arrange
            CountryAddRequest? country_add_request = new CountryAddRequest() { CountryName = "Poland" };
            CountryResponse country_resposne_from_add_request = await _countriesService.AddCountry(country_add_request);
            
            //Act
            CountryResponse? actual_country_response_from_get = await _countriesService.GetCountryByCountryID(country_resposne_from_add_request.CountryID);

            //Assert
            Assert.Equal(country_resposne_from_add_request, actual_country_response_from_get);
        }

        #endregion
    }
}
