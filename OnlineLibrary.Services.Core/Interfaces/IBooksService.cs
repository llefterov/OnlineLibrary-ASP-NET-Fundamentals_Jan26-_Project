using OnlineLibrary.Data.Models;
using OnlineLibrary.Web.ViewModels.Books;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IBooksService
    {
        //Task<Book?> GetBookAsync(Guid id);

        Task<IEnumerable<BooksAllViewModel>> GetAllBooksOrderedByTitleThenByGenreAscAsync(string? userId);

        Task<BookDetailsViewModel> GetBookDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync();

        Task<bool> IsBookAddedByUserAsync(string? userId, System.Guid bookId);

        Task<bool> IsBookAddedToUserCollectionAsync(string? userId, System.Guid bookId);
    }
}
