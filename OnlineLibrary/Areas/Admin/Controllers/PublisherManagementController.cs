using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.ViewModels.Publisher;
using static OnlineLibrary.Services.CustomMappers.PublisherMappers;

namespace OnlineLibrary.Web.Areas.Admin.Controllers
{
    public class PublisherManagementController : BaseAdminController
    {
        private readonly IPublisherManagementService publisherManagementService;
        private readonly ILogger<PublisherManagementController> logger;

        public PublisherManagementController(IPublisherManagementService publisherManagementService,
            ILogger<PublisherManagementController> logger)
        {
            this.publisherManagementService = publisherManagementService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var publishers = await publisherManagementService.GetAllPublishersAsync();
            return View(publishers);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new PublisherAddViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(PublisherAddViewModel inputModel)
        {
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            var modelDto = new PublisherAddDto { Name = inputModel.Name };

            try
            {
                await publisherManagementService.AddNewPublisherAsync(modelDto);
                return RedirectToAction(nameof(Manage));
            }
            catch (PublisherAlreadyExistsException ex)
            {
                logger.LogWarning(ex, "Attempt to add existing publisher {Name}", inputModel.Name);
                ModelState.AddModelError(nameof(PublisherAddViewModel.Name), ex.Message);
                return View(inputModel);
            }
            catch (PublisherCreateException ex)
            {
                logger.LogError(ex, "Error while adding publisher.");
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

            var modelDto = await publisherManagementService.GetNewPublisherForEditByIdAsync(id);
            if (modelDto == null)
            {
                logger.LogWarning("Attempt to edit non-existing publisher with id {PublisherId}", id);
                return NotFound();
            }

            return View(new PublisherEditViewModel { Id = modelDto.Id, Name = modelDto.Name });
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, PublisherEditViewModel inputModel)
        {
            if (id == Guid.Empty || id != inputModel.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                var serviceModel = new PublisherAllDto { Id = id, Name = inputModel.Name };
                var isUpdated = await publisherManagementService.UpdateNewPublisherAsync(id, serviceModel);
                if (!isUpdated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (PublisherUpdateExeption ex)
            {
                logger.LogError(ex, "Error while updating publisher {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the publisher. Please try again.");
                return View(inputModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while updating publisher {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");
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

            var publisherDto = await publisherManagementService.GetPublisherNewDeleteDetailsAsync(id);
            if (publisherDto == null)
            {
                logger.LogWarning("Attempt to delete non-existing publisher {PublisherId}", id);
                return NotFound();
            }

            return View(MapPublisherDeleteDtoToPublisherDeleteViewModel(publisherDto));
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
                var isDeleted = await publisherManagementService.DeletePublisherByIdAsync(id);
                if (!isDeleted)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (PublisherDeleteException ex)
            {
                logger.LogWarning(ex, "Cannot delete publisher {PublisherId} — has associated books.", id);
                ModelState.AddModelError(string.Empty, "Cannot delete a publisher that has associated books. Please remove the associations first.");

                var publisherDto = await publisherManagementService.GetPublisherNewDeleteDetailsAsync(id);
                if (publisherDto == null)
                {
                    return NotFound();
                }

                return View(MapPublisherDeleteDtoToPublisherDeleteViewModel(publisherDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting publisher {PublisherId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

                var publisherDto = await publisherManagementService.GetPublisherNewDeleteDetailsAsync(id);
                if (publisherDto == null)
                {
                    return NotFound();
                }

                return View(MapPublisherDeleteDtoToPublisherDeleteViewModel(publisherDto));
            }
        }
    }
}
