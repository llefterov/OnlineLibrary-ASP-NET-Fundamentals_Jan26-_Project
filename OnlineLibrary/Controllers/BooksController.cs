using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Books;

namespace OnlineLibrary.Web.Controllers
{
    public class BooksController : BaseController
    {
        private readonly OnlineLibraryDbContext dbContext;
        private readonly IBooksService booksService;
        private readonly ILogger<BooksController> logger;
        public BooksController(OnlineLibraryDbContext dbContext, IBooksService booksService, ILogger<BooksController> logger)
        {
            this.dbContext = dbContext;
            this.booksService = booksService;
            this.logger = logger;
        }

        [HttpGet]
        public async Task<IActionResult> All()
        {
            var userId = GetUserId();

            var allBooks = await booksService.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId);



            return View(allBooks);
        }


        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Details(Guid id)
        {
            var bookDetails = await booksService.GetBookDetailsByIdAsync(id);
            if (bookDetails == null)
            {
                logger.LogWarning("Book with ID {BookId} not found.", id);
                return RedirectToAction("All");
            }
            var userId = GetUserId();
            var isAddedByUser = await booksService.IsBookAddedByUserAsync(userId, id);
            var isAddedToUserCollection = await booksService.IsBookAddedToUserCollectionAsync(userId, id);

            // Assuming bookDetails has only one item since it's by ID
            bookDetails.IsAddedByUser = isAddedByUser;
            bookDetails.IsAddedToUserCollection = isAddedToUserCollection;

            return View(bookDetails);
        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Create()
        {
            var (publishers, authors) = await booksService.GetAuthorsAndPublishersAsync();
            ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
            ViewBag.Authors = new SelectList(authors, "Id", "FullName");

            var model = await booksService.GetBookCreateViewModelAsync();

            return View(model);
        }

        [HttpPost]
        [Authorize]
        public async Task<IActionResult> Create(BookCreateViewModel model)
        {
            if (!ModelState.IsValid)
            {
                var (publishers, authors) = await booksService.GetAuthorsAndPublishersAsync();
                ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
                ViewBag.Authors = new SelectList(authors, "Id", "FullName");

                var createModel = await booksService.GetBookCreateViewModelAsync();

                return View(createModel);
            }

            string? userId = GetUserId();

            try
            {
                await booksService.CreateBookAsync(model, userId);

            return RedirectToAction("All");
            }
            catch (Exception e)
            {
                logger.LogError(e, "An error occurred while creating a book with name {BookTitle}", model.Title);

                return View(model);
            }

        }

        [HttpGet]
        [Authorize]
        public async Task<IActionResult> Favorites()
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            IEnumerable<BookFavoritesViewModel> models = await booksService.GetFavoriteBooksAsync(userId);

            return View(models);
        }

        [HttpPost]
        public async Task<IActionResult> Save(Guid id)
        {
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            await booksService.SaveFevBookAsync(id, userId);

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
            string? userId = GetUserId();
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            await booksService.RemoveFevBookAsync(id, userId);
            return RedirectToAction("Favorites");
        }



    }
    //var(publishers, authors) = await booksService.GetAuthorsAndPublishersAsync();
    //ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
    //ViewBag.Authors = new SelectList(authors, "Id", "FullName");

}


