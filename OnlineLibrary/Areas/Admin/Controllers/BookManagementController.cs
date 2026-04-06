using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Web.ViewModels.Books;
using static OnlineLibrary.Services.CustomMappers.BookMappers;

namespace OnlineLibrary.Web.Areas.Admin.Controllers
{
    public class BookManagementController : BaseAdminController
    {
        private readonly IBookManagementService bookManagementService;
        private readonly ILogger<BookManagementController> logger;

        public BookManagementController(IBookManagementService bookManagementService,
            ILogger<BookManagementController> logger)
        {
            this.bookManagementService = bookManagementService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> Manage()
        {
            var allBooksDto = await bookManagementService.GetAllBooksForAdminDtoAsync();
            var model = allBooksDto.Select(MapBookAllDtoToBooksAllViewModel);
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await PopulateDropdownsAsync();
            var modelDto = await bookManagementService.GetBookDtoCreateViewModelAsync();
            return View(MapBookCreateDtoToBookCreateViewModel(modelDto));
        }

        [HttpPost]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            await PopulateDropdownsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Guid userId = GetUserId();
            var modelDto = MapBookCreateViewModelToBookCreateDto(model);

            try
            {
                await bookManagementService.CreateDtoBookAsync(modelDto, userId);
                return RedirectToAction(nameof(Manage));
            }
            catch (PublisherDoesntExistException ex)
            {
                logger.LogWarning(ex, "Selected publisher does not exist.");
                ModelState.AddModelError(nameof(model.PublisherId), "Selected publisher does not exist.");
                return View(model);
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "One or more selected authors are invalid.");
                ModelState.AddModelError(nameof(model.AuthorIds), "One or more selected authors are invalid.");
                return View(model);
            }
            catch (InvalidOperationException ex)
            {
                logger.LogWarning(ex, "Validation error while creating book.");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while creating book.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Edit(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            await PopulateDropdownsAsync();

            var modelDto = await bookManagementService.GetBookForAdminEditDtoAsync(id);
            if (modelDto == null)
            {
                logger.LogWarning("Book with ID {BookId} not found for admin edit.", id);
                return NotFound();
            }

            return View(MapBookEditDtoToBookEditViewModel(modelDto));
        }

        [HttpPost]
        public async Task<IActionResult> Edit([FromRoute] Guid id, BookEditViewModel model)
        {
            if (id == Guid.Empty || id != model.Id)
            {
                return BadRequest();
            }

            await PopulateDropdownsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var modelDto = MapBookEditViewModelToBookEditDto(model);

            try
            {
                var isEdited = await bookManagementService.EditBookForAdminDtoAsync(modelDto);
                if (!isEdited)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (PublisherDoesntExistException ex)
            {
                logger.LogWarning(ex, "Selected publisher does not exist.");
                ModelState.AddModelError(nameof(model.PublisherId), "Selected publisher does not exist.");
                return View(model);
            }
            catch (AuthorDoesntExistException ex)
            {
                logger.LogWarning(ex, "One or more selected authors are invalid.");
                ModelState.AddModelError(nameof(model.AuthorIds), "One or more selected authors are invalid.");
                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while editing book {BookId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var bookDto = await bookManagementService.GetBookAdminDeleteDetailsDtoAsync(id);
            if (bookDto == null)
            {
                logger.LogWarning("Book with ID {BookId} not found for admin delete.", id);
                return NotFound();
            }

            return View(MapBookDeleteDtoToBookDeleteViewModel(bookDto));
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            try
            {
                var isDeleted = await bookManagementService.DeleteBookForAdminDtoAsync(id);
                if (!isDeleted)
                {
                    return NotFound();
                }

                return RedirectToAction(nameof(Manage));
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while deleting book {BookId}.", id);
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

                var bookDto = await bookManagementService.GetBookAdminDeleteDetailsDtoAsync(id);
                if (bookDto == null)
                {
                    return NotFound();
                }

                return View(MapBookDeleteDtoToBookDeleteViewModel(bookDto));
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Restore([FromRoute] Guid id)
        {
            if (id == Guid.Empty)
            {
                return BadRequest();
            }

            var isRestored = await bookManagementService.RestoreBookForAdminDtoAsync(id);
            if (!isRestored)
            {
                logger.LogWarning("Book with ID {BookId} not found for admin restore.", id);
                return NotFound();
            }

            return RedirectToAction(nameof(Manage));
        }

        private async Task PopulateDropdownsAsync()
        {
            var (publishers, authors) = await bookManagementService.GetAllAuthorsAndPublishersAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
            ViewBag.Authors = new SelectList(authors, "Id", "FullName");
        }
    }
}
