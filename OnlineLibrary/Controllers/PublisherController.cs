using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Core.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Author;
using OnlineLibrary.Web.ViewModels.Publisher;

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
            var model = await publisherService.GetPublisherAllAsync();

            return View(model);
        }


        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] int id)
        {
            var publisher = await publisherService.GetPublisherByIdAsync(id);

            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var model = publisherService.GetEmtyPublisherFormModelAsync();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PublisherAddViewModel inputModel)
        {
            var model = publisherService.GetEmtyPublisherFormModelAsync();
            model = inputModel;

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                await publisherService.AddPublisherAsync(inputModel);
                return RedirectToAction("All", "Publisher");
            }
            catch (PublisherAlreadyExistsException ex)
            {
                logger.LogWarning(ex, "Attempt to add existing publisher {Name}", inputModel.Name);
                // bind error to the FullName field so user sees the specific issue
                ModelState.AddModelError(nameof(PublisherAddViewModel.Name), ex.Message);
                return View(inputModel);
            }
            catch (PublisherCreateException ex)
            {
                logger.LogError(ex, "An error occurred while adding a new publisher.");
                ModelState.AddModelError(string.Empty, "An error occurred while adding the publisher. Please try again.");
                return View(inputModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while adding publisher.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");
                return View(inputModel);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Edit([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            var model = new PublisherEditViewModel();
            try
            {
                model = await publisherService.GetPublisherForEditByIdAsync(id);
            }
            catch (PublisherDoesntExistException ex)
            {
                logger.LogWarning(ex, "Attempt to edit non-existing publisher with id {PublisherId}", id);
                ModelState.AddModelError(string.Empty, "The publisher you are trying to edit does not exist.");
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while loading edit form for publisher with id {PublisherId}.", id);
                return StatusCode(500, "An unexpected error occurred. Please contact support.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(int id, PublisherEditViewModel inputModel)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                await publisherService.UpdatePublisherAsync(id, inputModel);
                return RedirectToAction("All", "Publisher");
            }
            catch (PublisherUpdateExeption ex)
            {
                logger.LogError(ex, "An error occurred while updating publisher with id {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the publisher. Please try again.");
                return View(inputModel);
            }
            catch
            {
                logger.LogError("Unexpected error while updating publisher with id {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the publisher. Please contact support.");
                return View(inputModel);
            }
        }


        [HttpGet]
        public async Task<IActionResult> Delete([FromRoute] int id)
        {
            var publisherToDelete = await publisherService.GetPublisherDeleteDetailsAsync(id);

            if (publisherToDelete == null)
            {
                return NotFound();
            }

            return View(publisherToDelete);

        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] int id)
        {
            if (id <= 0)
            {
                return BadRequest();
            }

            try
            {
                await publisherService.DeletePublisherAsync(id);
                return RedirectToAction("All", "Publisher");
            }
            catch (PublisherDoesntExistException)
            {
                logger.LogWarning("Attempt to delete non-existing publisher with id {PublisherId}", id);
                ModelState.AddModelError(string.Empty, "The publisher you are trying to delete does not exist.");

                return NotFound();

            }
            catch (PublisherDeleteException ex)
            {
                logger.LogWarning(ex, "Attempt to delete publisher with id {PublisherId} that has associated books.", id);
                ModelState.AddModelError(string.Empty, "Cannot delete a publisher that has associated books. Please remove the associations first.");
                return View("Delete", await publisherService.GetPublisherDeleteDetailsAsync(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting publisher with id {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the publisher. Please contact support.");
                return View("Delete", await publisherService.GetPublisherDeleteDetailsAsync(id));
            }
        }




    }
}
