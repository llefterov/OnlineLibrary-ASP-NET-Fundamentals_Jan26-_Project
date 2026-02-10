using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Services.Core.Interfaces;

namespace OnlineLibrary.Web.Controllers
{
    public class PublisherController : BaseController
    {
        private readonly OnlineLibraryDbContext dbContext;
        private readonly IPublisherService publisherService;
        private readonly ILogger<PublisherController> logger;
        public PublisherController(OnlineLibraryDbContext dbContext, IPublisherService publisherService, ILogger<PublisherController> logger)
        {
            this.dbContext = dbContext;
            this.publisherService = publisherService;
            this.logger = logger;
        }


        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            var model = await publisherService.GetAllAsync();

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var publisher = await publisherService.GetByIdAsync(id);

            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }


    }
}
