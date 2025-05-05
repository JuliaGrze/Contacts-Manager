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
using RepositoryContracts;
using System.Linq.Expressions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        //private readonly IPersonsRepository _personsRepository;
        private readonly ITestOutputHelper _outputHelper;
        private readonly Mock<IPersonsRepository> _personsRepositoryMock;
        private readonly Mock<ICountriesRepository> _countriesRepositoryMock;
        private readonly IFixture _fixture;

        //constructor
        //public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        //{
        //    _outputHelper = testOutputHelper;
        //    _fixture = new Fixture();

        //    // Tworzenie mocków
        //    _personsRepositoryMock = new Mock<IPersonsRepository>();

        //    // Przekazanie mocków do serwisu
        //    _personsRepository = _personsRepositoryMock.Object;

        //    var countriesInitialData = new List<Country>();
        //    var personsInitialData = new List<Person>();

        //    //options jak baza ma ddzialac
        //    var options = new DbContextOptionsBuilder<ApplicationDbContext>()
        //        .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
        //        .Options;

        //    //tworzenie bazy na podatwie stworzonych wyzej opcji
        //    var dbContext = new ApplicationDbContext(options);

        //    //dodanie danych startowych - dbSet
        //    dbContext.Countries.AddRange(countriesInitialData);
        //    dbContext.Persons.AddRange(personsInitialData);
        //    dbContext.SaveChanges();

        //    _countriesService = new CountriesService(null);
        //    _personService = new PersonsService(_personsRepository);
        //}

        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _outputHelper = testOutputHelper;
            _fixture = new Fixture();

            // Tworzenie mocków
            _personsRepositoryMock = new Mock<IPersonsRepository>();
            _countriesRepositoryMock = new Mock<ICountriesRepository>();

            // Utwórz serwis używając zmockowanego repozytorium
            _countriesService = new CountriesService(_countriesRepositoryMock.Object); 

            // Utwórz serwis używając zmockowanego repozytorium
            _personService = new PersonsService(_personsRepositoryMock.Object);
        }

        #region AddPeron

        //When we supply null value as PersonAddRequest, it should throw ArgumentNullExpection
        [Fact]
        public async Task AddPerson_NullPerson_ToBeArgumentNullException()
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
        public async Task AddPerson_PeersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(prop => prop.PersonName, null as string)
                .Create();

            Person person = personAddRequest.ToPerson();

            //Mock the repository
            //When PersonRepository.AddPerson is called, it has to return the same "person" object
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            //Assert
            Func<Task> action = async () =>
            {
                await _personService.AddPerson(personAddRequest);
            };

            await action.Should().ThrowAsync<ArgumentException>();
        }

        //When we supply proper Person details, it should insert the person into person list and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public async Task AddPerson_FullPersonDetails_ToBeSuccessful()
        {
            //Arrange
            PersonAddRequest? personAddRequest = _fixture.Build<PersonAddRequest>()
                .With(temp => temp.Email, "someemail@example.com")
                .Create();

            Person person = personAddRequest.ToPerson();
            PersonResponse person_response_expected = person.ToPersonResponse();

            // if we supply any argumentt value to the AddPerson method, it should return the same return value
            // Ustawienie mocka, aby metoda Add zwróciła odpowiednią osobę
            //Jeśli ktoś w testach wywoła Add() na mocku repozytorium z jakimkolwiek obiektem Person, to zwróć ten obiekt jako wynik działania metody Add.
            _personsRepositoryMock.Setup(temp => temp.AddPerson(It.IsAny<Person>()))
            .ReturnsAsync(person); // Zwróci dodaną osobę

            //Act
            PersonResponse person_response_from_add = await _personService.AddPerson(personAddRequest);
            person_response_expected.PersonID = person_response_from_add.PersonID;

            //Assert
            //Assert.True(person_response_from_add.PersonID != Guid.Empty);
            person_response_from_add.PersonID.Should().NotBe(Guid.Empty);
            person_response_from_add.Should().Be(person_response_expected);
        }

        #endregion

        #region GetAllPersons

        [Fact]
        //The GetAllPersons() should return an empty list by default
        public async Task GetAllPersons_EmptyList_ToBeEmpty()
        {
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(new List<Person>());

            //Act
            List<PersonResponse> person_response_from_get = await _personService.GetAllPersons();

            //Assert
            //Assert.Empty(person_response_from_get);
            person_response_from_get.Should().BeEmpty();    
        }


        [Fact]
        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        public async Task GetAllPersons_WithFiewPersons_ToSucessful()
        {
            //Arrange
            List<Person> people = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person1@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person2@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_from_expected = people.Select(person => person.ToPersonResponse()).ToList();


            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach(PersonResponse person_response_from_add in person_response_list_from_expected)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Mock
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(people);

            //Act
            List<PersonResponse> person_reponse_from_get = await _personService.GetAllPersons();

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_get)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            person_reponse_from_get.Should().BeEquivalentTo(person_response_list_from_expected);
        }

        #endregion

        #region GetPersonByPersonID

        [Fact]
        //If we supply null as PersonID, it should return null as PersonResponse
        public async Task GetPersonByPersonID_NullPersonID_ToBeNull()
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
        public async Task GetPersonByPersonID_WithPersonID_ToBeSucessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "email@sample.com")
                .With(temp => temp.Country, null as Country)
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            //Repostory mock
            //Jeśli ktoś wywoła GetPersonById(some_id) na tym mocku, to zwróć obiekt person — tak, jakby pochodził z bazy danych
            _personsRepositoryMock.Setup(temp => temp.GetPersonById(person.PersonID))
                .ReturnsAsync(person);

            //Act
            PersonResponse? person_response_from_get = await _personService.GetPersonByPersonID(person.PersonID);

            //Assert
            //Assert.Equal(person_response_from_add, person_response_from_get);
            person_response_from_get.Should().Be(person_response_expected);
        }


        #endregion

        #region GetFilteredPersons

        [Fact]
        //If the search text is empty and search by is "PersonName", it should return all persons
        public async Task GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            List<Person> people = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person1@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person2@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_from_expected = people.Select(person => person.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(people);

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_expected)
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
            person_reponse_from_search.Should().BeEquivalentTo(person_response_list_from_expected);

        }

        [Fact]
        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        public async Task GetFilteredPersons_SearchByPersonName_ToBeSucessful()
        {
            //Arrange
            List<Person> people = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person1@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person2@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_from_expected = people.Select(person => person.ToPersonResponse()).ToList();

            _personsRepositoryMock.Setup(temp => temp.GetFilteredPersons(It.IsAny<Expression<Func<Person, bool>>>()))
                .ReturnsAsync(people);

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_expected)
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
            person_reponse_from_search.Should().BeEquivalentTo(person_response_list_from_expected);

        }

        #endregion

        #region GetSortedPersons


        [Fact]
        //When we sort based on PersonNmae in DESC, it should return person list in descending way
        public async Task GetSortedPersons_SearchByPersonName_ToBeSucessful()
        {
            //Arrange
            List<Person> people = new List<Person>()
            {
                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person1@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create(),

                _fixture.Build<Person>()
                    .With(temp => temp.Email, "person2@example.com")
                    .With(temp => temp.Country, null as Country)
                    .Create()
            };

            List<PersonResponse> person_response_list_from_expected = people.Select(person => person.ToPersonResponse()).ToList();

            //mock
            _personsRepositoryMock.Setup(temp => temp.GetAllPersons())
                .ReturnsAsync(people);

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_expected)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

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
        public async Task UpdatePerson_NullPerson_ToBeArgumentNullException()
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
        public async Task UpdatePerson_EmptyPerson_ToBeArgumentException()
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
        public async Task UpdatePerson_PersonNameIsNull_ToBeArgumentException()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.PersonName, null as string)
                .With(temp => temp.Country, null as Country)
                .Create();

            //mock
            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            PersonResponse person_response_from_add = person.ToPersonResponse();

            PersonUpdateRequest? person_update_request = person_response_from_add.ToPersonUpdateRequest();
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
        public async Task UpdatePerson_PersonFullDetails_ToBeSucessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.Gender, GenderOptions.Male.ToString())
                .Create();

            PersonResponse person_response_expected = person.ToPersonResponse();

            PersonUpdateRequest? person_update_request = person_response_expected.ToPersonUpdateRequest();
            
            //mock
            _personsRepositoryMock.Setup(temp => temp.UpdatePerson(It.IsAny<Person>()))
                .ReturnsAsync(person);

            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            //Act
            PersonResponse person_response_from_update = await _personService.UpdatePerson(person_update_request);
            //Assert

            //Assert.Equal(person_response_from_get, person_response_from_update);
            person_response_from_update.Should().Be(person_response_expected);
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
        public async Task DeletePerson_ValidPersonID_ToBeSucessful()
        {
            //Arrange
            Person person = _fixture.Build<Person>()
                .With(temp => temp.Email, "emai2l@sample.com")
                .With(temp => temp.Country, null as Country)
                .With(temp => temp.PersonName, "Julia")
                .With(temp => temp.Gender, "Female")
                .Create();

            //mock
            _personsRepositoryMock.Setup(temp => temp.DeletePersonByPersonID(It.IsAny<Guid>()))
                .ReturnsAsync(true);

            _personsRepositoryMock.Setup(temp => temp.GetPersonById(It.IsAny<Guid>()))
                .ReturnsAsync(person);

            //Act
            bool isDeleted = await _personService.DeletePerson(person.PersonID);

            //Assert
            //Assert.True(isDeleted);
            isDeleted.Should().BeTrue();
        }

        #endregion
    }
}
