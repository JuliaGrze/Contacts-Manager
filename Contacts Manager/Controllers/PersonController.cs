using Microsoft.AspNetCore.Mvc;

namespace Contacts_Manager.Controllers
{
    public class PersonController : Controller
    {
        [Route("persons/index")]
        [Route("/")]
        public IActionResult Index()
        {
            return View();
        }
    }
}
