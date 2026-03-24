using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.CodeAnalysis.CSharp.Syntax;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Services.Models.Publisher;
using OnlineLibrary.Web.ViewModels.Books;
using OnlineLibrary.Web.ViewModels.Publisher;
using System.Globalization;
using System.Runtime.Serialization;

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
        public async Task<IActionResult> All()
        {
            var userId = GetUserId();

            var allBooksDto = await booksService.GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(userId);
            var allBooksViewModel = allBooksDto.Select(MapBookAllViewModel);

            return View(allBooksViewModel);
        }

        [HttpGet]
        public async Task<IActionResult> MyBooks()
        {
            var userId = GetUserId();

            if (userId == Guid.Empty)
            {
                return View();
            }

            var myBooksDto = await booksService.GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(userId);
            var myBooksViewModel = myBooksDto.Select(MapBookAllViewModel);

            return View(myBooksViewModel);
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> Details(Guid id)
        {
            var bookDetailsDto = await booksService.GetBookDtoDetailsByIdAsync(id);
            if (bookDetailsDto == null)
            {
                logger.LogWarning("Book with ID {BookId} not found.", id);
                return RedirectToAction("All");
            }
            var userId = GetUserId();
            var isAddedByUser = await booksService.IsBookDtoAddedByUserAsync(userId, id);
            var isAddedToUserCollection = await booksService.IsBookDtoAddedToUserCollectionAsync(userId, id);

            // Assuming bookDetails has only one item since it's by ID
            bookDetailsDto.IsAddedByUser = isAddedByUser;
            bookDetailsDto.IsAddedToUserCollection = isAddedToUserCollection;

            var bookDetails = MapBookDetailsViewModel(bookDetailsDto);

            return View(bookDetails);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            await AddPublishersAndAuthirsListsAsync();
            var modelDto = await booksService.GetBookDtoCreateViewModelAsync();
            var model = MapBookCreateViewModel(modelDto);

            return View(model);
        }

        [HttpPost]
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

            var modelDto = MapBookCreateDto(model);

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
        public async Task<IActionResult> Favorites()
        {
            Guid userId = GetUserId();
            if (userId == Guid.Empty)
            {
                return RedirectToAction("Login", "Account");
            }

            var bookFavoritesDtos = await booksService.GetFavoriteBooksDtoAsync(userId);
            var bookFavoritesViewModels = bookFavoritesDtos.Select(MapBookFavoritesDtoToViewModel);

            return View(bookFavoritesViewModels);
        }

        [HttpPost]
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

        //[HttpGet]
        //public async Task<IActionResult> Edit(Guid id)
        //{
        //    await AddPublishersAndAuthirsListsAsync();

        //    Guid userId = GetUserId();
        //    if (userId == Guid.Empty)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    try
        //    {
        //        var model = await booksService.GetBookForEditAsync(id, userId);

        //        return View(model);
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Unauthorized();
        //    }
        //    catch (ArgumentException)
        //    {
        //        return NotFound();
        //    }
        //}

        //[HttpPost]
        //public async Task<IActionResult> Edit([FromRoute] Guid id, BookEditViewModel model)
        //{
        //    if (id != model.Id || id == Guid.Empty)
        //    {
        //        return BadRequest();
        //    }

        //    Guid userId = GetUserId();
        //    if (userId == Guid.Empty)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    await AddPublishersAndAuthirsListsAsync();

        //    if (!ModelState.IsValid)
        //    {
        //        return View(model);
        //    }

        //    try
        //    {
        //        await booksService.EditBookAsync(model, userId);
        //    }
        //    catch (PublisherDoesntExistException ex)
        //    {
        //        logger.LogWarning(ex, "Selected publisher does not exist.");
        //        ModelState.AddModelError(nameof(model.PublisherId), "Selected publisher does not exist.");
        //        return View(model);
        //    }
        //    catch (AuthorDoesntExistException ex)
        //    {
        //        logger.LogWarning(ex, "One or more selected authors are invalid.");
        //        ModelState.AddModelError(nameof(model.AuthorIds), "One or more selected authors are invalid.");
        //        return View(model);
        //    }
        //    catch (UnauthorizedAccessException ex)
        //    {
        //        logger.LogError(ex, "Unauthorized access while editing book.");
        //        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

        //        return View(model);
        //    }
        //    catch (Exception ex)
        //    {
        //        logger.LogError(ex, "Unexpected error while adding book.");
        //        ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please contact support.");

        //        return View(model);
        //    }

        //    return RedirectToAction("Details", new { id = model.Id });
        //}

        //[HttpGet]
        //public async Task<IActionResult> Delete(Guid id, string? returnUrl = null)
        //{
        //    Guid userId = GetUserId();
        //    if (userId == Guid.Empty)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    var book = await booksService.GetBookDeleteDetailsAsync(id, userId);

        //    // Preserve the returnUrl so the POST can redirect back to the previous page
        //    var referer = Request.Headers["Referer"].ToString();
        //    ViewData["ReturnUrl"] = returnUrl ?? (!string.IsNullOrEmpty(referer) ? referer : null);

        //    return View(book);
        //}

        //[HttpPost, ActionName("Delete")]
        //[ValidateAntiForgeryToken]
        //public async Task<IActionResult> DeleteConfirmed(Guid id, string? returnUrl)
        //{
        //    Guid userId = GetUserId();
        //    if (userId == Guid.Empty)
        //    {
        //        return RedirectToAction("Login", "Account");
        //    }

        //    try
        //    {
        //        await booksService.DeleteBookAsync(id, userId);
        //    }
        //    catch (UnauthorizedAccessException)
        //    {
        //        return Unauthorized();
        //    }
        //    catch (ArgumentException)
        //    {
        //        return NotFound();
        //    }

        //    // Prefer an explicit returnUrl, otherwise fall back to the Referer header if provided.
        //    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
        //    {
        //        // Avoid redirecting back to a details page for a deleted book
        //        if (returnUrl.Contains("/Details", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return RedirectToAction("All");
        //        }

        //        return Redirect(returnUrl);
        //    }

        //    var referer = Request.Headers["Referer"].ToString();
        //    if (!string.IsNullOrEmpty(referer) && Url.IsLocalUrl(referer))
        //    {
        //        if (referer.Contains("/Details", StringComparison.OrdinalIgnoreCase))
        //        {
        //            return RedirectToAction("All");
        //        }

        //        return Redirect(referer);
        //    }
        //    return RedirectToAction("MyBooks");
        //}

        private async Task AddPublishersAndAuthirsListsAsync()
        {
            var (publishers, authors) = await booksService.GetAllAuthorsAndPublishersAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
            ViewBag.Authors = new SelectList(authors, "Id", "FullName");
        }

        private static BooksAllViewModel MapBookAllViewModel(BookAllDto booksAllDto)
        {
            return new BooksAllViewModel
            {
                Id = booksAllDto.Id,
                Title = booksAllDto.Title,
                Genre = booksAllDto.Genre,
                GenreName = booksAllDto.Genre.ToString(),
                Rating = booksAllDto.Rating,
                CoverUrl = booksAllDto.CoverUrl ?? string.Empty,
                AddedByUserName = booksAllDto.AddedByUserName, // null-safe
                PublisherId = booksAllDto.PublisherId,
                PublisherName = booksAllDto.PublisherName,
                IsAddedByUser = booksAllDto.IsAddedByUser, // Assuming this is already set correctly in the DTO
                IsAddedToUserCollection = booksAllDto.IsAddedToUserCollection // Assuming this is already set correctly in the DTO
            };

        }

        private static BookDetailsViewModel MapBookDetailsViewModel(BookDetailsDto bookDetailsDto)
        {
            var booksDetailsViewModel = new BookDetailsViewModel
            {
                Id = bookDetailsDto.Id,
                Title = bookDetailsDto.Title,
                Description = bookDetailsDto.Description,
                Genre = bookDetailsDto.Genre,
                GenreName = bookDetailsDto.GenreName,
                IsRead = bookDetailsDto.IsRead,
                DateRead = bookDetailsDto.DateRead,
                Rating = bookDetailsDto.Rating,
                CoverUrl = bookDetailsDto.CoverUrl ?? string.Empty,
                DateAdded = bookDetailsDto.DateAdded,
                PublisherId = bookDetailsDto.PublisherId,
                PublisherName = bookDetailsDto.PublisherName,
                AuthorsName = bookDetailsDto.AuthorsName,
                AddedByUserName = bookDetailsDto.AddedByUserName, // safe access
                IsAddedByUser = bookDetailsDto.IsAddedByUser,
                IsAddedToUserCollection = bookDetailsDto.IsAddedToUserCollection
            };

            if (bookDetailsDto.IsRead == false)
            {
                bookDetailsDto.DateRead = null;
            }

            return booksDetailsViewModel;
        }

        private static BookCreateViewModel MapBookCreateViewModel(BookCreateDto bookCreateDto)
        {
            return new BookCreateViewModel
            {
                Title = bookCreateDto.Title,
                Description = bookCreateDto.Description,
                Genre = bookCreateDto.Genre,
                IsRead = bookCreateDto.IsRead,
                DateRead = bookCreateDto.DateRead,
                Rating = bookCreateDto.Rating,
                CoverUrl = bookCreateDto.CoverUrl,
                DateAdded = bookCreateDto.DateAdded,
                PublisherId = bookCreateDto.PublisherId,
                AddedByUserId = bookCreateDto.AddedByUserId,
                AuthorIds = bookCreateDto.AuthorIds
            };
        }



        private static BookCreateDto MapBookCreateDto(BookCreateViewModel bookCreateViewModel)
        {
            return new BookCreateDto
            {
                Title = bookCreateViewModel.Title,
                Description = bookCreateViewModel.Description,
                Genre = bookCreateViewModel.Genre,
                IsRead = bookCreateViewModel.IsRead,
                DateRead = bookCreateViewModel.DateRead,
                Rating = bookCreateViewModel.Rating,
                CoverUrl = bookCreateViewModel.CoverUrl,
                DateAdded = bookCreateViewModel.DateAdded,
                PublisherId = bookCreateViewModel.PublisherId,
                AddedByUserId = bookCreateViewModel.AddedByUserId,
                AuthorIds = bookCreateViewModel.AuthorIds
            };
        }

        private static BookFavoritesViewModel MapBookFavoritesDtoToViewModel(BookFavoritesDto favBookDto)
        {
            return new BookFavoritesViewModel
            {
                Id = favBookDto.Id,
                Title = favBookDto.Title,
                CoverUrl = favBookDto.CoverUrl ?? string.Empty
            };
        }
    }
}
