using Microsoft.AspNetCore.Mvc;
using ServiceContracts;
using ServiceContracts.DTO;
using ServiceContracts.Enums;

namespace Contacts_Manager.Controllers
{
    [Route("[controller]")]
    public class PersonsController : Controller
    {
        //private fields
        private readonly IPersonsService _personsService;
        private readonly ICountriesService _countriesService;

        //constructor
        public PersonsController(IPersonsService personsService, ICountriesService countriesService)
        {
            _personsService = personsService;
            _countriesService = countriesService;
        }

        //person list

        [Route("[action]")]
        [Route("/")]
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderEnum sortOrder = SortOrderEnum.ASC )
        {
            //Search
            //Actual Name - Display name - 
            ViewBag.SearchFields = new Dictionary<string, string>()
            {
                {nameof(PersonResponse.PersonName), "Person Name" },
                {nameof(PersonResponse.Email), "Email" },
                {nameof(PersonResponse.DateOfBirth), "Date of Birth" },
               
                {nameof(PersonResponse.Gender), "Gender" },
                {nameof(PersonResponse.CountryID), "Country" },
                {nameof(PersonResponse.Address), "Address" }
            };
            Console.WriteLine("fgh");
            //Search
            List<PersonResponse> persons = _personsService.GetFilteredPersons(searchBy, searchString);
            
            
            //Save search fields after searching
            ViewBag.CurrentSearchBy = searchBy;
            ViewBag.CurrentSearchString = searchString;

            //Sort
            List<PersonResponse> sortedPersons = _personsService.GetSortedPersons(persons, sortBy, sortOrder);
            ViewBag.CurrentSortBy = sortBy;
            ViewBag.CurremtSortOrder = sortOrder.ToString();

            return View(sortedPersons); //Views/Persons/Index.cshtml
        }

        //Executes when th euser clicks on "Create Person" hyperlink 
        [Route("create")]
        [HttpGet]
        public IActionResult Create()
        {
            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries;
            return View();
        }

        [HttpPost]
        [Route("[action]")]
        public IActionResult Create(PersonAddRequest personAddRequest)
        {
            //check if person details is valid
            //Sprawdza, czy dane przesłane z formularza są poprawne według walidacji ustawionej w klasie PersonAddRequest
            if (!ModelState.IsValid)
            {
                List<CountryResponse> countries = _countriesService.GetAllCountries();
                ViewBag.Countries = countries;
                ViewBag.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

                return View();
            }
            //call the service method
            PersonResponse personResponse = _personsService.AddPerson(personAddRequest);
            //navigate to Index() action method (it makes another get request to "persons/index")
            return RedirectToAction("Index");
        }
    }
}
