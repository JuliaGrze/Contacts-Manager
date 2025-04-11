using Entities;
using ServiceContracts.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceContracts.DTO
{
    /// <summary>
    /// Represents DTO class that is used as return type of most methods of Persons Service
    /// </summary>
    public class PersonResponse
    {
        public Guid PersonID { get; set; }
        public string? PersonName { get; set; }
        public string? Email { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }
        public Guid? CountryID { get; set; }
        public string? Country { get; set; }
        public string? Address { get; set; }
        public bool? ReceiveNewsLetters { get; set; }
        public double? Age { get; set; }

        /// <summary>
        /// Compares the current object data with the parametr object
        /// </summary>
        /// <param name="obj">The personResponse Object to compare</param>
        /// <returns>True or false, indicating whether all person details are  mached with the specified parametr object</returns>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;

            if(obj.GetType() != typeof(PersonResponse)) return false;

            PersonResponse? other = (PersonResponse)obj;

            return
                other.PersonID == PersonID &&
                other.PersonName == PersonName &&
                other.Email == Email &&
                other.DateOfBirth == DateOfBirth &&
                other.Gender == Gender &&
                other.CountryID == CountryID &&
                other.Address == Address &&
                other.ReceiveNewsLetters == ReceiveNewsLetters;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public override string ToString()
        {
            return "PersonID: " + PersonID + ", PersonName: " + PersonName + ", Email: " + Email + ", DateOfBirth: " + DateOfBirth?.ToString("dd MM yyyy") + ", Gender: " + Gender + ", CountryID: " + CountryID + ", Address: " + Address + ", ReceiveNewsLetters: " + ReceiveNewsLetters;
        }

        public PersonUpdateRequest ToPersonUpdateRequest()
        {
            return new PersonUpdateRequest()
            {
                PersonID = PersonID,
                PersonName = PersonName,
                Email = Email,
                DateOfBirth = DateOfBirth,
                Gender = Enum.TryParse(typeof(GenderOptions), Gender, true, out var result)
                    ? (GenderOptions?)result
                    : null,
                CountryID = CountryID,
                Address = Address,
                ReceiveNewsLetters = ReceiveNewsLetters ?? false
            };
        }
    }

    public static class PersonExtensions
    {
        /// <summary>
        /// An existing method to convert an object of Person class into PersonRespone class
        /// </summary>
        /// <param name="person">The Person object to convert</param>
        /// <returns>Returns the converted PersonResponse object</returns>
        public static PersonResponse ToPersonResponse(this Person person)
        {
            return new PersonResponse()
            {
                PersonID = person.PersonID,
                PersonName = person.PersonName,
                Email = person.Email,
                DateOfBirth = person.DateOfBirth,
                Gender = person.Gender,
                CountryID = person.CountryID,
                Address = person.Address,
                ReceiveNewsLetters = person.ReceiveNewsLetters,
                Age = person.DateOfBirth != null
                        ? Math.Round((DateTime.Now - person.DateOfBirth.Value).TotalDays / 365.25)
                        : null
            };
        }
    }
}
