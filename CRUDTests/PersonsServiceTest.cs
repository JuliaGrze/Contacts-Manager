using Entities;
using Microsoft.EntityFrameworkCore;
using Moq;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;
using AutoFixture;
using AutoFixture.Kernel;
using FluentAssertions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _outputHelper;
        private readonly IFixture _fixture;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _outputHelper = testOutputHelper;
            _fixture = new Fixture();

            var countriesInitialData = new List<Country>();
            var personsInitialData = new List<Person>();

            //options jak baza ma ddzialac
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;

            //tworzenie bazy na podatwie stworzonych wyzej opcji
            var dbContext = new ApplicationDbContext(options);

            //dodanie danych startowych - dbSet
            dbContext.Countries.AddRange(countriesInitialData);
            dbContext.Persons.AddRange(personsInitialData);
            dbContext.SaveChanges();

            _countriesService = new CountriesService(dbContext);
            _personService = new PersonsService(dbContext, _countriesService);
        }

        #region AddPeron

        //When we supply null value as PersonAddRequest, it should throw ArgumentNullExpection
        [Fact]
        public async Task AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //deletgae - funkjca anonimowa
            Func<Task> action = async () =>
            {
                //Act
                await _personService.AddPerson(personAddRequest);
            };

            //Assert
            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        //When we supply null value as PersonName, it should throw ArgumentExpection
        [Fact]
        public async Task AddPerson_PeersonNameIsNull()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(prop => prop.PersonName, null as string)
                .Create();

            //Assert
            Func<Task> action = async () =>
            {
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When we supply proper Person details, it should insert the person into person list and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_Proper()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someemail@example.com")
                .Create();

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);

            List<PersonResponse> persons_list = await _personService.GetAllPersons();

            //Assert
            //Assert.True(person_response_from_add.PersonID != Guid.Empty);
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);

            //Assert.Contains(person_response_from_add, persons_list);
            persons_list.Should().Contain(person_response_from_add);
        }

        #endregion

        #region GetAllPersons

        [Fact]
        //The GetAllPersons() should return an empty list by default
        public async Task GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> person_response_from_get = await _personService.GetAllPersons();

            //Assert
            //Assert.Empty(person_response_from_get);
            person_response_from_get.Should().BeEmpty();    
        }


        [Fact]
        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        public async Task GetAllPersons_AddFiewPersons()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_add_request_2 = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response_1 = await _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person1@exmample.com")
                .Create();
            
            PersonAddRequest person_add_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "person2@exmample.com")
                .Create();

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach(PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(await _personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach(PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_get = await _personService.GetAllPersons();

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_get)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            person_reponse_from_get.Should().BeEquivalentTo(person_response_list_from_add);
        }

        #endregion

        #region GerPersonByPersonID

        [Fact]
        //If we supply null as PersonID, it should return null as PersonResponse
        public async Task GerPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid personID = Guid.Empty;

            //Act
            PersonResponse? person_response_from_get_person_by_id = await _personService.GetPersonByPersonID(personID);

            //Assert
            //Assert.Null(person_response_from_get_person_by_id);
            person_response_from_get_person_by_id.Should().BeNull();
        }
        
        [Fact]
        //If we supply a valid person id, it should return a valid person details as PersonResponse object
        public async Task GerPersonByPersonID_WithPersonID()
        {
            //Arrange
            CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response_from_get_ = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "email@sample.com")
                .Create();

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);

            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonID(person_response_from_add.PersonID);

            //Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);
            person_response_from_get.Should().Be(person_response_from_add);
        }


        #endregion

        #region GetFilteredPersons

        [Fact]
        //If the search text is empty and search by is "PersonName", it should return all persons
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_add_request_2 = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response_1 = await _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "email1@sample.com")
                .Create();
            PersonAddRequest person_add_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .Create();

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(await _personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_search)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            person_reponse_from_search.Should().BeEquivalentTo(person_response_list_from_add);

        }

        [Fact]
        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        public async Task GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_add_request_2 = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response_1 = await _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "email1@sample.com")
                .With(temp => temp.PersonName, "mary")
                .With(temp => temp.CountryID, country_response_1.CountryID)
                .Create();
            PersonAddRequest person_add_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.PersonName, "julia")
                .With(temp => temp.CountryID, country_response_2.CountryID)
                .Create();

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(await _personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_search = await _personService.GetFilteredPersons(nameof(Person.PersonName), "mary");

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_search)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            //foreach (PersonResponse person in person_response_list_from_add)
            //{
            //    if(person.PersonName != null)
            //        if(person.PersonName.Contains("mary", StringComparison.OrdinalIgnoreCase))
            //            Assert.Contains(person, person_reponse_from_search);
            //}
            person_reponse_from_search.Should().OnlyContain(temp =>
                temp.PersonName.Contains("mary", StringComparison.OrdinalIgnoreCase));

        }

        #endregion

        #region GetSortedPersons

        private async Task<List<PersonResponse>> MakePersonResponseList()
        {
            CountryAddRequest country_add_request_1 = _fixture.Create<CountryAddRequest>();
            CountryAddRequest country_add_request_2 = _fixture.Create<CountryAddRequest>();

            CountryResponse country_response_1 = await _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = await _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "email1@sample.com")
                .With(temp => temp.PersonName, "julia")
                .Create();
            PersonAddRequest person_add_request_2 = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.PersonName, "wersow")
                .Create();

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(await _personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }
            return person_response_list_from_add;
        }

        [Fact]
        //When we sort based on PersonNmae in DESC, it should return person list in descending way
        public async Task GetSortedPersons_SearchByPersonName()
        {
            //Arrange
            List<PersonResponse> person_response_list_from_add = await MakePersonResponseList();


            List<PersonResponse> allPersons = await _personService.GetAllPersons();

            //Act
            List<PersonResponse> person_reponse_from_sort = await _personService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderEnum.DESC);

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in person_reponse_from_sort)
            {
                _outputHelper.WriteLine(person.ToString());
            }

            //person_response_list_from_add = person_response_list_from_add.OrderByDescending(person => person.PersonName).ToList();

            //Assert
            //for (int i = 0; i < person_response_list_from_add?.Count; i++)
            //{
            //    Assert.Equal(person_response_list_from_add[i], person_reponse_from_sort[i]);
            //}

            person_reponse_from_sort.Should().BeInDescendingOrder(temp => temp.PersonName);

        }

        #endregion

        #region UpdatePerson

        [Fact]
        //When we supply null as PersonUpdateRequest, it should throw ArgumenNullException
        public async Task UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = null;

            //Assert
            //await Assert.ThrowsAsync<ArgumentNullException>(async () =>
            //{
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});  
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(person_update_request);
            };

            await action.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        //When we supply invalid person id, it should throw ArgumenException
        public async Task UpdatePerson_EmptyPerson()
        {
            //Arrange
            PersonUpdateRequest person_update_request = _fixture.Create<PersonUpdateRequest>();

            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async () =>
            //{
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        //When personName is null, it should throw ArgumentException
        public async Task UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .Create();
            PersonResponse person_response = await _personService.AddPerson(person_add_request);

            PersonUpdateRequest? person_update_request = person_response.ToPersonUpdateRequest();
            person_update_request.PersonName = null;

            //Assert
            //await Assert.ThrowsAsync<ArgumentException>(async() =>
            //{
            //    //Act
            //    await _personService.UpdatePerson(person_update_request);
            //});
            Func<Task> action = async () =>
            {
                await _personService.UpdatePerson(person_update_request);
            };
            await action.Should().ThrowAsync<ArgumentException>();
        }

        [Fact]
        //First, add a new person and try to update the person name and email
        public async Task UpdatePerson_PersonFullDetails()
        {
            //Arrange
            CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.CountryID, country_response_from_add.CountryID)
                .Create();
            PersonResponse person_response = await _personService.AddPerson(person_add_request);

            PersonUpdateRequest? person_update_request = person_response.ToPersonUpdateRequest();
            person_update_request.PersonName = "Weronika";
            person_update_request.Email = "wwjj@onet.pl";

            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(person_update_request);
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonID(person_response_from_update.PersonID);
            //Assert

            //Assert.Equal(person_response_from_get, person_response_from_update);
            person_response_from_update.Should().Be(person_response_from_get);
        }

        #endregion

        #region DeletePerson

        [Fact]
        //If you supply an invalid PersonID, it should return true
        public async Task DeletePerson_InvalidPersonID()
        {
            //Act
            bool isDeleted = await _personService.DeletePerson(Guid.NewGuid());

            //Assert
            //Assert.False(isDeleted);
            isDeleted.Should().BeFalse();
        }

        [Fact]
        //If you supply a valid PersonID, it should return true
        public async Task DeletePerson_ValidPersonID()
        {
            //Arrange
            CountryAddRequest country_add_request = _fixture.Create<CountryAddRequest>();
            CountryResponse country_response_from_add = await _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.CountryID, country_response_from_add.CountryID)
                .Create();
            PersonResponse person_response_from_add = await _personService.AddPerson(person_add_request);

            //Act
            bool isDeleted = await _personService.DeletePerson(person_response_from_add.PersonID);

            //Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}
