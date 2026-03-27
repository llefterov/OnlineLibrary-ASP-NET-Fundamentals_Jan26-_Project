using Microsoft.AspNetCore.Mvc;

namespace OnlineLibrary.Web.Areas.Admin.Controllers
{
    public class BookManagementController : Controller
    {
        public IActionResult Index()
        {
            return View();
        }
    }
}
