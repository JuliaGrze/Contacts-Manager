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
using Moq;
using AutoFixture;
using FluentAssertions;
using Azure.Core;
using RepositoryContracts;


namespace CRUDTests
{
    public class CountriesServiceTest
    {
        private readonly ICountriesService _countriesService;
        private readonly IFixture _fixture;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;

        //public CountriesServiceTest()
        //{
        //    _fixture = new Fixture();
        //    var countriesInitialData = new List<Country>();

        //    //Tworzysz specjalny "budowniczy" (builder), który pomoże skonfigurować jak będzie działać Twój ApplicationDbContext
        //    //Options = sposób, w jaki baza będzie działać (np. że będzie w pamięci, a nie na prawdziwym serwerze).
        //    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) //// nowa baza dla każdego testu
        //        .Options;

        //    //Tworzysz bazę danych na podstawie tych wcześniej przygotowanych instrukcji (options).
        //    //Czyli: "Masz tu gotowe ustawienia, teraz utwórz bazę według nich".
        //    ApplicationDbContext dbContext = new ApplicationDbContext(options);

        //    //dodanie danych startowych - dbSet
        //    //tworzona jest nowa baza danych w pamięci (In-Memory Database), ale ta baza istnieje tylko podczas testów
        //    dbContext.Countries.AddRange(countriesInitialData);
        //    dbContext.SaveChanges();

        //    _countriesService = new CountriesService(null);
        //}

        public CountriesServiceTest()
        {
            _fixture = new Fixture();

            _countriesRepositoryMock = new Mock<ICountriesRepository>(); // ✅ utwórz mock repozytorium

            _countriesService = new CountriesService(_countriesRepositoryMock.Object); // ✅ użyj mocka
        }

        #region AddCountry

        //When CountryAddRequest is null, it should ArgumentNullException
        [Fact]
        public async Task AddCountry_NullCountry()
        {
            //Arrange
            CountryAddRequest? request = null;

            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            //{
            //    //Act
            //    await _countriesService.AddCountry(request);
            //});
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request); ;
            };
            await action.Should().ThrowAsync<ArgumentNullException>();

        }

        //When CountryName is null, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_CountryNameIsNull()
        {
            //Arrange
            CountryAddRequest? request = _fixture.Build<CountryAddRequest>()
                .With(temp => temp.CountryName, null as string)
                .Create();

            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () =>
            //{
            //    //Act
            //    await _countriesService.AddCountry(request);
            //});
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When CountryName is duplicate, it should throw ArgumentException
        [Fact]
        public async Task AddCountry_DuplicateCountryName_ShouldThrowArgumentException()
        {
            // Arrange
            var request1 = new CountryAddRequest() { CountryName = "Poland" };
            var request2 = new CountryAddRequest() { CountryName = "Poland" };

            var countryEntity = new Country() { CountryName = "Poland" };

            // 1. Gdy dodajesz pierwszy raz – nic nie istnieje
            _countriesRepositoryMock.Setup(repo => repo.GetCountryByCountryName("Poland"))
                .ReturnsAsync((Country?)null);

            // 2. Gdy dodajesz drugi raz – symulujemy, że taki kraj już istnieje
            _countriesRepositoryMock.SetupSequence(repo => repo.GetCountryByCountryName("Poland"))
                .ReturnsAsync((Country?)null)       // dla request1
                .ReturnsAsync(countryEntity);       // dla request2

            _countriesRepositoryMock.Setup(repo => repo.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync((Country c) => c);

            // Act
            Func<Task> action = async () =>
            {
                await _countriesService.AddCountry(request1);
                await _countriesService.AddCountry(request2); // powinien rzucić wyjątek
            };

            // Assert
            await action.Should().ThrowAsync<ArgumentException>();
               
        }



        //When you supply proper CountryName, it should insert (add) the country to the existing list of countries
        [Fact]
        public async Task AddCountry_ProperCountryDetails()
        {
            //Arrange
            CountryAddRequest request = _fixture.Create<CountryAddRequest>();

            _countriesRepositoryMock.Setup(repo => repo.AddCountry(It.IsAny<Country>()))
                .ReturnsAsync((Country c) => c);

            //Act
            CountryResponse response = await _countriesService.AddCountry(request);

            //Assert
            //Assert.True(response.CountryID != Guid.Empty);
            response.CountryID.Should().NotBe(Guid.Empty);
        }

        #endregion


        #region GetAllCountries

        //The list of countries should be empty by default (before adding any countries)
        [Fact]
        public async Task GetAllCountries_EmptyList()
        {
            // Arrange
            _countriesRepositoryMock.Setup(repo => repo.GetAllCountries())
                .ReturnsAsync(new List<Country>()); // Mockowanie pustej listy

            // Act
            List<CountryResponse> actual_country_response_list = await _countriesService.GetAllCountries();

            // Assert
            actual_country_response_list.Should().BeEmpty(); // Oczekujemy pustej listy
        }


        // It checks whether all countries in the request list are present in the response after being added.
        [Fact]
        public async Task GetAllCountries_AddFewCountries()
        {
            //Arrange
            var countries = new List<Country>
            {
                 new Country { CountryID = Guid.NewGuid(), CountryName = "Poland" },
                new Country { CountryID = Guid.NewGuid(), CountryName = "Germany" },
                new Country { CountryID = Guid.NewGuid(), CountryName = "France" }
            };


            //Act
            List<CountryResponse> countries_list_from_add_country = countries.Select(temp => temp.ToCountryResponse()).ToList();

            _countriesRepositoryMock.Setup(temp => temp.GetAllCountries())
                .ReturnsAsync(countries);

            List<CountryResponse> actualCountryResponseList = await _countriesService.GetAllCountries();

            //read each element from countries_list_from_add_country
            //foreach (CountryResponse expected_country in countries_list_from_add_country)
            //{
            //    //Assert
            //    Assert.Contains(expected_country, actualCountryResponseList);
            //}
            countries_list_from_add_country.Should().BeEquivalentTo(actualCountryResponseList);
            
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
            //Assert.Null(country_resposne_from_get_method);
            country_resposne_from_get_method.Should().BeNull(); 
        }

        [Fact]
        //If we supply a valid countryID, it should return the matching country details as CountryResult object
        public async Task GetCountryByCountryID_ValidCountryID()
        {
            //Arrange
            CountryAddRequest? country_add_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_resposne_from_add_request = await _countriesService.AddCountry(country_add_request);
            
            //Act
            CountryResponse? actual_country_response_from_get = await _countriesService.GetCountryByCountryID(country_resposne_from_add_request.CountryID);

            //Assert
            //Assert.Equal(country_resposne_from_add_request, actual_country_response_from_get);
            actual_country_response_from_get.Should().BeEquivalentTo(actual_country_response_from_get);
        }

        #endregion
    }
}
