using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Author;
using OnlineLibrary.Web.ViewModels.Author;

namespace OnlineLibrary.Web.Controllers
{
    public class AuthorController : BaseController
    {
        private readonly IAuthorService authorService;
        private readonly ILogger<AuthorController> logger;
        public AuthorController(IAuthorService authorService, ILogger<AuthorController> logger)
        {
            this.authorService = authorService;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All()
        {
            var model = await authorService.GetAllAuthorsForViewModelAsync();

            var authorsList = model
                .Select(a => new AuthorAllViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName
                })
                .ToList();
            return View(authorsList);
        }

        [HttpGet]
        public async Task<IActionResult> Details([FromRoute] Guid id)
        {
            var authorDetailsDto = await authorService
                .GetAuthorDetailsByIdAsync(id);

            if (authorDetailsDto == null)
            {
                return NotFound();
            }

            var authorModel = new AuthorDetailsViewModel
            {
                Id = authorDetailsDto.Id,
                FullName = authorDetailsDto.FullName,
                BooksWithPublisherName = authorDetailsDto.BooksWithPublisherName
                    .Select(b => new AuthorBookViewModel
                    {
                        Id = b.Id,
                        Title = b.Title,
                        CoverUrl = b.CoverUrl,
                        Rating = b.Rating,
                        DateAdded = b.DateAdded,
                        GenreName = b.GenreName,
                        PublisherName = b.PublisherName,
                        Description = b.Description
                    })
                   .ToList()
            };

            if (authorModel == null)
            {
                return NotFound();
            }
            return View(authorModel);
        }

        [HttpGet]
        public IActionResult Add()
        {
            var model = new AuthorAddViewModel();
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Add(AuthorAddViewModel inputModel)
        {
            if (!ModelState.IsValid)
            {
                return View(inputModel);
            }

            try
            {
                var serviceModel = new AuthorsAllDto
                {
                    FullName = inputModel.FullName
                };

                await authorService.AddNewAuthorAsync(serviceModel);
                return RedirectToAction("All", "Author");
            }
            catch (AuthorAlreadyExistsException ex)
            {
                logger.LogWarning(ex, "Attempt to add existing author {FullName}", inputModel.FullName);
                ModelState.AddModelError(nameof(AuthorAddViewModel.FullName), ex.Message);
                return View(inputModel);
            }
            catch (AuthorCreateException ex)
            {
                logger.LogError(ex, "An error occurred while adding a new author.");
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

            var model = new AuthorEditViewModel();

            try
            {
                var modelDto = await authorService
                    .GetNewAuthorForEditByIdAsync(id);
                
                model = new AuthorEditViewModel
                {
                    Id = modelDto.Id,
                    FullName = modelDto.FullName
                };
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "Attempt to edit non-existing author with id {AuthorId}", id);
                ModelState.AddModelError(string.Empty, "The author you are trying to edit does not exist.");
                return NotFound();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while loading edit form for author with id {AuthorId}.", id);
                return StatusCode(500, "An unexpected error occurred. Please contact support.");
            }

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Guid id, AuthorEditViewModel inputModel)
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
                var serviceModel = new AuthorsAllDto
                {
                    Id = inputModel.Id,
                    FullName = inputModel.FullName
                };

                await authorService.UpdateNewAuthorAsync(id, serviceModel);
                return RedirectToAction("All", "Author");
            }
            catch (AuthorUpdateExeption ex)
            {
                logger.LogError(ex, "An error occurred while updating author with id {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An error occurred while updating the author. Please try again.");
                return View(inputModel);
            }
            catch
            {
                logger.LogError("Unexpected error while updating author with id {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while updating the author. Please contact support.");
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
                var authorToDeleteDto = await authorService.GetAuthorNewDeleteDetailsAsync(id);

                var authorToDelete = new AuthorDeleteViewModel
                {
                    Id = authorToDeleteDto.Id,
                    FullName = authorToDeleteDto.FullName,
                    BooksAuthors = authorToDeleteDto.BooksAuthors
                };

                return View(authorToDelete);
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "Attempt to get delete details for non-existing author with id {AuthorId}", id);
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
                await authorService.DeleteAuthorByIdAsync(id);
                return RedirectToAction("All", "Author");
            }
            catch (AuthorDoesntExistException)
            {
                logger.LogWarning("Attempt to delete non-existing author with id {AuthorId}", id);
                ModelState.AddModelError(string.Empty, "The author you are trying to delete does not exist.");

                return NotFound();
            }
            catch (AuthorDeleteException ex)
            {
                logger.LogWarning(ex, "Attempt to delete author with id {AuthorId} that has associated books.", id);
                ModelState.AddModelError(string.Empty, "Cannot delete an author that has associated books. Please remove the associations first.");
                return View("Delete", await authorService.GetAuthorNewDeleteDetailsAsync(id));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting author with id {AuthorId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred while deleting the author. Please contact support.");
                return View("Delete", await authorService.GetAuthorNewDeleteDetailsAsync(id));
            }
        }
    }
}
