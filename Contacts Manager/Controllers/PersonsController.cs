using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
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
        public IActionResult Index(string searchBy, string? searchString, string sortBy = nameof(PersonResponse.PersonName), SortOrderEnum sortOrder = SortOrderEnum.ASC)
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
            //ViewBag.Countries = countries;
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

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
                ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });
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

        //Edit person details
        [HttpGet]
        [Route("[action]/{personID}")] //Eg: /persons/edit/1
        public IActionResult Edit(Guid personID)
        {
            PersonResponse personResponse = _personsService.GetPersonByPersonID(personID);
            if(personResponse == null) 
                return RedirectToAction("Index");

            List<CountryResponse> countries = _countriesService.GetAllCountries();
            ViewBag.Countries = countries.Select(temp =>
                new SelectListItem() { Text = temp.CountryName, Value = temp.CountryID.ToString() });

            PersonUpdateRequest personUpdateRequest = personResponse.ToPersonUpdateRequest();
            return View(personUpdateRequest);
        }

        [HttpPost]
        [Route("[action]/{personID}")]
        public IActionResult Edit(PersonUpdateRequest personUpdateRequest)
        {
            PersonResponse? personResponse = _personsService.GetPersonByPersonID(personUpdateRequest.PersonID);

            if(personResponse == null )
                return RedirectToAction("Index");

            //if personResponse is not null and valid
            if (ModelState.IsValid)
            {
               PersonResponse updayedPerson = _personsService.UpdatePerson(personUpdateRequest);
                return RedirectToAction("Index");
            }

            //if is not valid
            ViewBag.Errors = ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage)
                    .ToList();

            return View();

        }
    }
}
