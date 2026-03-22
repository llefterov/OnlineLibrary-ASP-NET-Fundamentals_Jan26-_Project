using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.GCommon;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.ViewModels.Publisher;
using System.Globalization;
using System.Runtime.Serialization;
using static OnlineLibrary.GCommon.ApplicationConstants;

namespace OnlineLibrary.Web.Controllers
{
    public class PublisherController : BaseController
    {
        private readonly IPublisherService publisherService;
        private readonly ILogger<PublisherController> logger;
        public PublisherController(IPublisherService publisherService, ILogger<PublisherController> logger)
        {
            this.publisherService = publisherService;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            var model = await publisherService
                .GetAllPublishersAsync();
            var publishersList = model
                .Select(p => new PublisherAllViewModel
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToList();

            return View(publishersList);
        }

        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            var publisherDto = await publisherService.GetPublisherDetailsByIdAsync(id);



            var publisher = new PublisherDetailsViewModel
                {
                    Id = publisherDto.Id,
                    Name = publisherDto.Name,
                    BooksWithAuthorName = publisherDto.BooksWithAuthorName
                        .Select(b => new PublisherBookViewModel
                        {
                            Id = b.Id,
                            Title = b.Title,
                            CoverUrl = b.CoverUrl ?? string.Empty,
                            Rating = b.Rating,
                            DateAdded = b.DateAdded,
                            GenreName = b.GenreName,
                            AuthorsName = b.AuthorsName,
                            Description = b.Description
                        })
                        .ToList()
                };

            if (publisher == null)
            {
                return NotFound();
            }
            return View(publisher);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var model = new PublisherAddViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(PublisherAddViewModel inputModel)
        {
            var modelDto = publisherService.GetEmptyPublisherViewModelAsync();
            
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            modelDto = new PublisherAddDto
            {
                Name = inputModel.Name
            };
                

            try
            {
                await publisherService.AddNewPublisherAsync(modelDto);
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
        public async Task<IActionResult> Edit([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var model = new PublisherEditViewModel();
            try
            {
                var modelDto = await publisherService.GetNewPublisherForEditByIdAsync(id);
                model = new PublisherEditViewModel
                {
                    Id = modelDto.Id,
                    Name = modelDto.Name
                };
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
        public async Task<IActionResult> Edit(Guid id, PublisherEditViewModel inputModel)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                var serviceModel = new PublisherAllDto
                {
                    Id = id,
                    Name = inputModel.Name
                };      

                await publisherService.UpdateNewPublisherAsync(id, serviceModel);
                return RedirectToAction("All", "Publisher");
            }
            catch (PublisherDoesntExistException ex)
            {
                logger.LogError(ex, "Publisher does not exist");
                ModelState.AddModelError(string.Empty, "Publisher does not exist");
                return View(inputModel);
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
        public async Task<IActionResult> Delete([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {

                var publisherToDelete = await publisherService.GetPublisherNewDeleteDetailsAsync(id);
                return View(publisherToDelete);
            }
            catch (PublisherDoesntExistException)
            {
                logger.LogWarning("Attempt to delete non-existing publisher with id {PublisherId}", id);
                ModelState.AddModelError(string.Empty, "The publisher you are trying to delete does not exist.");

                return NotFound();
            }
        }

        [HttpPost, ActionName("Delete")]
        public async Task<IActionResult> DeleteConfirmed([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                await publisherService.DeletePublisherByIdAsync(id);
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
                return View("Delete", await publisherService.GetPublisherNewDeleteDetailsAsync(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting publisher with id {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the publisher. Please contact support.");
                return View("Delete", await publisherService.GetPublisherNewDeleteDetailsAsync(id));
            }
        }
    }
}
