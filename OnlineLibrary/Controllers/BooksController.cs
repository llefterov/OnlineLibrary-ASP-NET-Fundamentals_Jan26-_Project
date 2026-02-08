using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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



        //var(publishers, authors) = await booksService.GetAuthorsAndPublishersAsync();
        //ViewBag.Publishers = new SelectList(publishers, "Id", "Name");
        //ViewBag.Authors = new SelectList(authors, "Id", "FullName");


    }
}
