using Entities;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xunit.Abstractions;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonsService _personService;
        private readonly ICountriesService _countriesService;
        private readonly ITestOutputHelper _outputHelper;

        //constructor
        public PersonsServiceTest(ITestOutputHelper testOutputHelper)
        {
            _personService = new PersonsService();
            _countriesService = new CountriesService();
            _outputHelper = testOutputHelper;
        }

        #region AddPeron

        //When we supply null value as PersonAddRequest, it should throw ArgumentNullExpection
        [Fact]
        public void AddPerson_NullPerson()
        {
            //Arrange
            PersonAddRequest? personAddRequest = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _personService.AddPerson(personAddRequest);
            });
        }

        //When we supply null value as PersonName, it should throw ArgumentExpection
        [Fact]
        public void AddPerson_PeersonNameIsNull()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { PersonName = null};

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _personService.AddPerson(personAddRequest);
            });
        }

        //When we supply proper Person details, it should insert the person into person list and it should return an object of PersonResponse, which includes with the newly generated person id
        [Fact]
        public void AddPerson_Proper()
        {
            //Arrange
            PersonAddRequest? personAddRequest = new PersonAddRequest() { 
                PersonName = "Julia",
                Email = "person@example.com",
                Address = "sample address",
                CountryID = Guid.NewGuid(),
                Gender = GenderOptions.Male,
                DateOfBirth = DateTime.Parse("2000-01-01"),
                ReceiveNewsLetters = true
            };

            //Act
            PersonResponse person_response_from_add = _personService.AddPerson(personAddRequest);

            List<PersonResponse> persons_list = _personService.GetAllPersons();

            //Assert
            Assert.True(person_response_from_add.PersonID != Guid.Empty);

            Assert.Contains(person_response_from_add, persons_list);
        }

        #endregion

        #region GetAllPersons

        [Fact]
        //The GetAllPersons() should return an empty list by default
        public void GetAllPersons_EmptyList()
        {
            //Act
            List<PersonResponse> person_response_from_get = _personService.GetAllPersons();

            //Assert
            Assert.Empty(person_response_from_get);
        }


        [Fact]
        //First, we will add few persons; and then when we call GetAllPersons(), it should return the same persons that were added
        public void GetAllPersons_AddFiewPersons()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = new CountryAddRequest() { CountryName = "UK" };
            CountryAddRequest country_add_request_2 = new CountryAddRequest() { CountryName = "Poland" };

            CountryResponse country_response_1 = _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = new PersonAddRequest()
            {
                PersonName = "Person1",
                Email = "person1@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Female,
                CountryID = country_response_1.CountryID,
                Address = "addres1",
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_add_request_2 = new PersonAddRequest()
            {
                PersonName = "Person2",
                Email = "person2@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Male,
                CountryID = country_response_2.CountryID,
                Address = "addres2",
                ReceiveNewsLetters = false
            };

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach(PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(_personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach(PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_get = _personService.GetAllPersons();

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_get)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            foreach (PersonResponse person in person_response_list_from_add)
            {
                Assert.Contains(person, person_reponse_from_get);
            }
            
        }

        #endregion

        #region GerPersonByPersonID

        [Fact]
        //If we supply null as PersonID, it should return null as PersonResponse
        public void GerPersonByPersonID_NullPersonID()
        {
            //Arrange
            Guid personID = Guid.Empty;

            //Act
            PersonResponse? person_response_from_get_person_by_id = _personService.GetPersonByPersonID(personID);

            //Assert
            Assert.Null(person_response_from_get_person_by_id);
        }

        [Fact]
        //If we supply a valid person id, it should return a valid person details as PersonResponse object
        public void GerPersonByPersonID_WithPersonID()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest()
            {
                CountryName = "Canada"
            };
            CountryResponse country_response_from_get_ = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest()
            {
                PersonName = "Person",
                Email = "person@example.com",
                Address = "addres",
                DateOfBirth = DateTime.Parse("2001-03-03"),
                CountryID = country_response_from_get_.CountryID,
                Gender = GenderOptions.Male,
                ReceiveNewsLetters = false
            };

            //Act
            PersonResponse person_response_from_add = _personService.AddPerson(person_add_request);

            PersonResponse? person_response_from_get = _personService.GetPersonByPersonID(person_response_from_add.PersonID);

            //Assert
            Assert.Equal(person_response_from_add, person_response_from_get);
        }


        #endregion

        #region GetFilteredPersons

        [Fact]
        //If the search text is empty and search by is "PersonName", it should return all persons
        public void GetFilteredPersons_EmptySearchText()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = new CountryAddRequest() { CountryName = "UK" };
            CountryAddRequest country_add_request_2 = new CountryAddRequest() { CountryName = "Poland" };

            CountryResponse country_response_1 = _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = new PersonAddRequest()
            {
                PersonName = "Person1",
                Email = "person1@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Female,
                CountryID = country_response_1.CountryID,
                Address = "addres1",
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_add_request_2 = new PersonAddRequest()
            {
                PersonName = "Person2",
                Email = "person2@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Male,
                CountryID = country_response_2.CountryID,
                Address = "addres2",
                ReceiveNewsLetters = false
            };

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(_personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_search = _personService.GetFilteredPersons(nameof(Person.PersonName), "");

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_search)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            foreach (PersonResponse person in person_response_list_from_add)
            {
                Assert.Contains(person, person_reponse_from_search);
            }

        }

        [Fact]
        //First we will add few persons; and then we will search based on person name with some search string. It should return the matching persons
        public void GetFilteredPersons_SearchByPersonName()
        {
            //Arrange
            CountryAddRequest country_add_request_1 = new CountryAddRequest() { CountryName = "UK" };
            CountryAddRequest country_add_request_2 = new CountryAddRequest() { CountryName = "Poland" };

            CountryResponse country_response_1 = _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = new PersonAddRequest()
            {
                PersonName = "Mary",
                Email = "person1@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Female,
                CountryID = country_response_1.CountryID,
                Address = "addres1",
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_add_request_2 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "person2@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Male,
                CountryID = country_response_2.CountryID,
                Address = "addres2",
                ReceiveNewsLetters = false
            };

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(_personService.AddPerson(person_request));
            }

            //print person_response_list_from_add
            _outputHelper.WriteLine("Expected:");
            foreach (PersonResponse person_response_from_add in person_response_list_from_add)
            {
                _outputHelper.WriteLine(person_response_from_add.ToString());
            }

            //Act
            List<PersonResponse> person_reponse_from_search = _personService.GetFilteredPersons(nameof(Person.PersonName), "mary");

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person_reponse_from_gett in person_reponse_from_search)
            {
                _outputHelper.WriteLine(person_reponse_from_gett.ToString());
            }

            //Assert
            foreach (PersonResponse person in person_response_list_from_add)
            {
                if(person.PersonName != null)
                    if(person.PersonName.Contains("mary", StringComparison.OrdinalIgnoreCase))
                        Assert.Contains(person, person_reponse_from_search);
            }

        }

        #endregion

        #region GetSortedPersons

        

        private List<PersonResponse> MakePersonResponseList()
        {
            CountryAddRequest country_add_request_1 = new CountryAddRequest() { CountryName = "UK" };
            CountryAddRequest country_add_request_2 = new CountryAddRequest() { CountryName = "Poland" };

            CountryResponse country_response_1 = _countriesService.AddCountry(country_add_request_1);
            CountryResponse country_response_2 = _countriesService.AddCountry(country_add_request_2);

            PersonAddRequest person_add_request_1 = new PersonAddRequest()
            {
                PersonName = "Mary",
                Email = "person1@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Female,
                CountryID = country_response_1.CountryID,
                Address = "addres1",
                ReceiveNewsLetters = true
            };
            PersonAddRequest person_add_request_2 = new PersonAddRequest()
            {
                PersonName = "Rahman",
                Email = "person2@exmample.com",
                DateOfBirth = DateTime.Parse("2003-03-03"),
                Gender = GenderOptions.Male,
                CountryID = country_response_2.CountryID,
                Address = "addres2",
                ReceiveNewsLetters = false
            };

            List<PersonAddRequest> person_add_requests = new List<PersonAddRequest>()
            {
                person_add_request_1, person_add_request_2
            };

            List<PersonResponse> person_response_list_from_add = new List<PersonResponse>();

            foreach (PersonAddRequest person_request in person_add_requests)
            {
                person_response_list_from_add.Add(_personService.AddPerson(person_request));
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
        public void GetSortedPersons_SearchByPersonName()
        {
            //Arrange
            List<PersonResponse> person_response_list_from_add = MakePersonResponseList();


            List<PersonResponse> allPersons = _personService.GetAllPersons();

            //Act
            List<PersonResponse> person_reponse_from_sort = _personService.GetSortedPersons(allPersons, nameof(Person.PersonName), SortOrderEnum.DESC);

            //print person_reponse_from_get
            _outputHelper.WriteLine("Actual:");
            foreach (PersonResponse person in person_reponse_from_sort)
            {
                _outputHelper.WriteLine(person.ToString());
            }

            person_response_list_from_add = person_response_list_from_add.OrderByDescending(person => person.PersonName).ToList();

            //Assert
            for (int i = 0; i < person_response_list_from_add?.Count; i++)
            {
                Assert.Equal(person_response_list_from_add[i], person_reponse_from_sort[i]);
            }

        }

        #endregion

        #region UpdatePerson

        [Fact]
        //When we supply null as PersonUpdateRequest, it should throw ArgumenNullException
        public void UpdatePerson_NullPerson()
        {
            //Arrange
            PersonUpdateRequest? person_update_request = null;

            //Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                //Act
                _personService.UpdatePerson(person_update_request);
            });  
        }

        [Fact]
        //When we supply invalid person id, it should throw ArgumenException
        public void UpdatePerson_EmptyPerson()
        {
            //Arrange
            PersonUpdateRequest person_update_request = new PersonUpdateRequest()
            {
                PersonID = Guid.NewGuid(),
            };

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _personService.UpdatePerson(person_update_request);
            });
        }

        [Fact]
        //When personName is null, it should throw ArgumentException
        public void UpdatePerson_PersonNameIsNull()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "Poland" };
            CountryResponse country_response_from_add = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "Julia", Email = "person@exmaple.com", 
            CountryID = country_response_from_add.CountryID };
            PersonResponse person_response = _personService.AddPerson(person_add_request);

            PersonUpdateRequest? person_update_request = person_response.ToPersonUpdateRequest();
            person_update_request.PersonName = null;

            //Assert
            Assert.Throws<ArgumentException>(() =>
            {
                //Act
                _personService.UpdatePerson(person_update_request);
            });
        }

        [Fact]
        //First, add a new person and try to update the person name and email
        public void UpdatePerson_PersonFullDetails()
        {
            //Arrange
            CountryAddRequest country_add_request = new CountryAddRequest() { CountryName = "Poland" };
            CountryResponse country_response_from_add = _countriesService.AddCountry(country_add_request);

            PersonAddRequest person_add_request = new PersonAddRequest() { PersonName = "Julia", Email = "person@exmaple.com", 
            CountryID = country_response_from_add.CountryID,
            Address = "addres1", DateOfBirth = DateTime.Parse("2003-02-03"), Gender = GenderOptions.Female, ReceiveNewsLetters = false};
            PersonResponse person_response = _personService.AddPerson(person_add_request);

            PersonUpdateRequest? person_update_request = person_response.ToPersonUpdateRequest();
            person_update_request.PersonName = "Weronika";
            person_update_request.Email = "wwjj@onet.pl";

            //Act
            PersonResponse person_response_from_update = _personService.UpdatePerson(person_update_request);
            PersonResponse? person_response_from_get = _personService.GetPersonByPersonID(person_response_from_update.PersonID);
            //Assert

            Assert.Equal(person_response_from_get, person_response_from_update);
        }

        #endregion
    }
}
