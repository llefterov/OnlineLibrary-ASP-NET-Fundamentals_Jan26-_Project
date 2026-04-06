using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.ViewModels.Author;
using static OnlineLibrary.Services.CustomMappers.AuthorMappers;

namespace OnlineLibrary.Web.Areas.Admin.Controllers
{
    public class AuthorManagementController : BaseAdminController
    {
        private readonly IAuthorManagementService authorManagementService;
        private readonly ILogger<AuthorManagementController> logger;

        public AuthorManagementController(IAuthorManagementService authorManagementService,
            ILogger<AuthorManagementController> logger)
        {
            this.authorManagementService = authorManagementService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var authors = await authorManagementService.GetAllAuthorsForViewModelAsync();
            return View(authors.AuthorsAllDtos);
        }

        [HttpGet]
        public IActionResult Add()
        {
            return View(new AuthorAddViewModel());
        }

        [HttpPost]
        public async Task<IActionResult> Add(AuthorAddViewModel inputModel)
        {
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            var serviceModel = new AuthorsAllDto { FullName = inputModel.FullName };

            try
            {
                await authorManagementService.AddNewAuthorAsync(serviceModel);
                return RedirectToAction(nameof(Manage));
            }
            catch (AuthorAlreadyExistsException ex)
            {
                logger.LogWarning(ex, "Attempt to add existing author {FullName}", inputModel.FullName);
                ModelState.AddModelError(nameof(AuthorAddViewModel.FullName), ex.Message);
                return View(inputModel);
            }
            catch (AuthorCreateException ex)
            {
                logger.LogError(ex, "Error while adding author.");
                ModelState.AddModelError(string.Empty, "An error occurred while adding the author. Please try again.");
                return View(inputModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while adding author.");
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

            var modelDto = await authorManagementService.GetNewAuthorForEditByIdAsync(id);
            if (modelDto == null)
            {
                logger.LogWarning("Attempt to edit non-existing author with id {AuthorId}", id);
                return NotFound();
            }

            return View(new AuthorEditViewModel { Id = modelDto.Id, FullName = modelDto.FullName });
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] Guid id, AuthorEditViewModel inputModel)
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
                var serviceModel = new AuthorsAllDto { Id = id, FullName = inputModel.FullName };
                var isUpdated = await authorManagementService.UpdateNewAuthorAsync(id, serviceModel);
                if (!isUpdated)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (AuthorUpdateExeption ex)
            {
                logger.LogError(ex, "Error while updating author {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the author. Please try again.");
                return View(inputModel);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while updating author {AuthorId}.", id);
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

            var authorDto = await authorManagementService.GetAuthorNewDeleteDetailsAsync(id);
            if (authorDto == null)
            {
                logger.LogWarning("Attempt to delete non-existing author {AuthorId}", id);
                return NotFound();
            }

            return View(MapAuthorDeleteDtoToAuthorDeleteViewModel(authorDto));
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
                var isDeleted = await authorManagementService.DeleteAuthorByIdAsync(id);
                if (!isDeleted)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (AuthorDeleteException ex)
            {
                logger.LogWarning(ex, "Cannot delete author {AuthorId} — has associated books.", id);
                ModelState.AddModelError(string.Empty, "Cannot delete an author that has associated books. Please remove the associations first.");

                var authorDto = await authorManagementService.GetAuthorNewDeleteDetailsAsync(id);
                if (authorDto == null)
                {
                    return NotFound();
                }

                return View(MapAuthorDeleteDtoToAuthorDeleteViewModel(authorDto));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting author {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

                var authorDto = await authorManagementService.GetAuthorNewDeleteDetailsAsync(id);
                if (authorDto == null)
                {
                    return NotFound();
                }

                return View(MapAuthorDeleteDtoToAuthorDeleteViewModel(authorDto));
            }
        }
    }
}
