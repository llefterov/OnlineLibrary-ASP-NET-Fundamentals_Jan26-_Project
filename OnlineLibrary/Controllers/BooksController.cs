using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Web.ViewModels.Books;

namespace OnlineLibrary.Web.Controllers
{
    public class BooksController : Controller
    {
        private readonly OnlineLibraryDbContext dbContext;
        public BooksController(OnlineLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }





        public IActionResult All()
        {
            var allBooks = dbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Publisher)
                .Include(b => b.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
                .AsNoTracking()
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.Genre,
                    b.isRead,
                    b.DateRead,
                    b.Rating,
                    b.CoverUrl,
                    b.DateAdded,
                    b.AddedByUser,
                    b.PublisherId,
                    PublisherName = b.Publisher.Name,
                    Authors = b.BooksAuthors.Select(ba => ba.Author.FullName)
                })
                .AsEnumerable()
                .Select(b => new BooksAllViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Description = b.Description,
                    Genre = b.Genre,
                    GenreName = b.Genre.ToString(),
                    isRead = b.isRead,
                    DateRead = b.DateRead,
                    Rating = b.Rating,
                    CoverUrl = b.CoverUrl,
                    DateAdded = b.DateAdded,
                    AddedByUserName = b.AddedByUser?.UserName,
                    PublisherId = b.PublisherId,
                    PublisherName = b.PublisherName,
                    AuthorsName = string.Join(", ", b.Authors)
                })
                .ToList();

            return View(allBooks);
        }
    }
}
