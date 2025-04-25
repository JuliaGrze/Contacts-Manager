using Microsoft.AspNetCore.Mvc;
using ServiceContracts;

namespace Contacts_Manager.Controllers
{
    [Route("[controller]")]
    public class CountriesController : Controller
    {
        //private fields
        private readonly ICountriesService _countriesService;

        //constructor
        public CountriesController(ICountriesService countriesService)
        {
            _countriesService = countriesService;
        }

        [Route("[action]")]
        [HttpGet]
        public IActionResult UploadFromExcel()
        {
            return View();
        }

        [Route("[action]")]
        [HttpPost]
        public async Task<IActionResult> UploadFromExcel(IFormFile excelFile)
        {
            if(excelFile == null || excelFile.Length == 0)
            {
                ViewBag.ErrorMessage = "Please select an xlsx file.";
                return View();
            }

            if(!Path.GetExtension(excelFile.FileName).Equals(".xlsx", StringComparison.OrdinalIgnoreCase))
            {
                ViewBag.ErrorMessage = "Unsupported file. 'xlsx' file is expected";
                return View();
            }

            try
            {
                int countriesCountInserted = await _countriesService.UploadCountriesFromExcelFile(excelFile);
                ViewBag.Message = $"{countriesCountInserted} countries Uploaded";
            }
            catch (Exception ex) 
            {
                ViewBag.ErrorMessage = ex.Message;
            }
            
            return View();
        }
    }
}
