using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Books;
using static OnlineLibrary.Services.CustomMappers.BookMappers;

namespace OnlineLibrary.Web.Controllers
{
    public class BooksController : BaseController
    {
        private readonly IBooksService booksService;
        private readonly ILogger<BooksController> logger;
        public BooksController(IBooksService booksService, ILogger<BooksController> logger)
        {
            this.booksService = booksService;
            this.logger = logger;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> All(string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1)
        {
            var userId = GetUserId();

            var (allBooksDto, totalPages) = await booksService.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(userId,  searchQuery, publisherFilter, genreFilter, pageNumber, pageSize: 5); 
            var allBooksViewModel = allBooksDto.Select(MapBookAllDtoToBooksAllViewModel);

            ViewData["SearchQuery"] = searchQuery;
            ViewData["PublisherFilter"] = publisherFilter;
            ViewData["GenreFilter"] = genreFilter;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;

            return View(allBooksViewModel);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> MyBooks(string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1)
        {
            var userId = GetUserId();

            if (userId == Guid.Empty)
            {
                var emptyBooksViewModel = Enumerable.Empty<BooksAllViewModel>();
                ViewData["SearchQuery"] = searchQuery;
                ViewData["PublisherFilter"] = publisherFilter;
                ViewData["GenreFilter"] = genreFilter;
                ViewData["CurrentPage"] = pageNumber;
                ViewData["TotalPages"] = 0;

                return View(emptyBooksViewModel);
            }

            var (myBooksDto, totalPages) = await booksService.GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(userId, searchQuery, publisherFilter, genreFilter, pageNumber, pageSize: 5);

            var myBooksViewModel = myBooksDto.Select(MapBookAllDtoToBooksAllViewModel);

            ViewData["SearchQuery"] = searchQuery;
            ViewData["PublisherFilter"] = publisherFilter;
            ViewData["GenreFilter"] = genreFilter;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;

            return View(myBooksViewModel);
        }

        [HttpGet("Books/Details/{id:guid}")]
        [HttpGet("Books/Details/{slug}/{id:guid}")]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Details(Guid id)
        {
            var bookDetailsDto = await booksService.GetBookDtoDetailsByIdAsync(id);
            if (bookDetailsDto == null)
            {
                logger.LogWarning("Book with ID {BookId} not found.", id);
                return NotFound();
            }
            var userId = GetUserId();
            var isAddedByUser = await booksService.IsBookDtoAddedByUserAsync(userId, id);
            var isAddedToUserCollection = await booksService.IsBookDtoAddedToUserCollectionAsync(userId, id);

            bookDetailsDto.IsAddedByUser = isAddedByUser;
            bookDetailsDto.IsAddedToUserCollection = isAddedToUserCollection;

            var bookDetails = MapBookDetailsDtoToBookDetailsViewModel(bookDetailsDto);

            return View(bookDetails);
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create()
        {
            await AddPublishersAndAuthirsListsAsync();
            var modelDto = await booksService.GetBookDtoCreateViewModelAsync();
            var model = MapBookCreateDtoToBookCreateViewModel(modelDto);

            return View(model);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            await AddPublishersAndAuthirsListsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return View();
            }

            var modelDto = MapBookCreateViewModelToBookCreateDto(model);

            try
            {
                await AddPublishersAndAuthirsListsAsync();

                await booksService.CreateDtoBookAsync(modelDto, userId);
                return RedirectToAction("All");
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
            catch (InvalidOperationException ex) // expected/validation errors from service
            {
                logger.LogWarning(ex, "Validation while creating book");
                ModelState.AddModelError(string.Empty, ex.Message);
                return View(model);
            }
            catch (Exception ex) // unexpected
            {
                logger.LogError(ex, "Unexpected error while creating book");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Favorites(string? searchQuery = null, int pageNumber = 1)
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var (bookFavoritesDtos, totalPages) = await booksService.GetFavoriteBooksDtoAsync(userId, searchQuery, pageNumber, pageSize: 5);
            var bookFavoritesViewModels = bookFavoritesDtos.Select(MapBookFavoritesDtoToBookFavoritesViewModel);

            ViewData["SearchQuery"] = searchQuery;
            ViewData["CurrentPage"] = pageNumber;
            ViewData["TotalPages"] = totalPages;

            return View(bookFavoritesViewModels);
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Save(Guid id)
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            await booksService.SaveFevBookDtoAsync(id, userId);

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
            {
                return Redirect(referer);
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager,User")]
        public async Task<IActionResult> Remove(Guid id)
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            await booksService.RemoveFevBookDtoAsync(id, userId);
            return RedirectToAction("Favorites");
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit(Guid id)
        {
            await AddPublishersAndAuthirsListsAsync();

            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var modelDto = await booksService.GetBookForEditDtoAsync(id, userId);

                if (modelDto == null)
                {
                    return NotFound();
                }

                var model = MapBookEditDtoToBookEditViewModel(modelDto);

                return View(model);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
            catch (ArgumentException)
            {
                return NotFound();
            }
        }

        [HttpPost]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Edit([FromRoute] Guid id, BookEditViewModel model)
        {
            if (id != model.Id || id == Guid.Empty)
            {
                return BadRequest();
            }

            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            await AddPublishersAndAuthirsListsAsync();

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var modelDto = MapBookEditViewModelToBookEditDto(model);

            try
            {
                var isEdited = await booksService.EditBookDtoAsync(modelDto, userId);
                if (!isEdited)
                {
                    return NotFound();
                }
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
            catch (UnauthorizedAccessException ex)
            {
                logger.LogError(ex, "Unauthorized access while editing book.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

                return View(model);
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Unexpected error while adding book.");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

                return View(model);
            }

            return RedirectToAction("Details", new { id = model.Id });
        }

        [HttpGet]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> Delete(Guid id, string? returnUrl = null)
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var bookDeleteDto = await booksService.GetBookDeleteDetailsDtoAsync(id, userId);

                if (bookDeleteDto == null)
                {
                    return NotFound();
                }

                // Preserve the returnUrl so the POST can redirect back to the previous page
                var referer = Request.Headers["Referer"].ToString();
                ViewData["ReturnUrl"] = returnUrl ?? (!string.IsNullOrEmpty(referer) ? referer : null);

                var bookViewModel = MapBookDeleteDtoToBookDeleteViewModel(bookDeleteDto);

                return View(bookViewModel);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }
        }

        [HttpPost, ActionName("Delete")]
        [Authorize(Roles = "Admin,Manager")]
        public async Task<IActionResult> DeleteConfirmed(Guid id, string? returnUrl)
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            try
            {
                var isDeleted = await booksService.DeleteBookDtoAsync(id, userId);
                if (!isDeleted)
                {
                    return NotFound();
                }
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized();
            }

            // Prefer an explicit returnUrl, otherwise fall back to the Referer header if provided.
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                // Avoid redirecting back to a details page for a deleted book
                if (returnUrl.Contains("/Details", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("All");
                }

                return Redirect(returnUrl);
            }

            var referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
            {
                if (referer.Contains("/Details", StringComparison.OrdinalIgnoreCase))
                {
                    return RedirectToAction("All");
                }

                return Redirect(referer);
            }
            return RedirectToAction("MyBooks");
        }

        private async Task AddPublishersAndAuthirsListsAsync()
        {
            var (publishers, authors) = await booksService.GetAllAuthorsAndPublishersAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
            ViewBag.Authors = new SelectList(authors, "Id", "FullName");
        }
    }
}
