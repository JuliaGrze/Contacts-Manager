using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.DTO.Enums;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CRUDTests
{
    public class PersonsServiceTest
    {
        //private fields
        private readonly IPersonsService _personService;

        //constructor
        public PersonsServiceTest()
        {
            _personService = new PersonsService();
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

        //[Fact]
        //public void GetAllPersons_EmptyList()
        //{
        //    //Act
        //    List<PersonResponse> person_response_from_get = _personService.GetAllPersons();

        //    //Assert
        //    Assert.Null(person_response_from_get);
        //}

        #endregion
    }
}
