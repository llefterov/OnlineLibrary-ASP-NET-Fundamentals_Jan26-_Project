using OnlineLibrary.Data.Models;
using OnlineLibrary.Web.ViewModels.Books;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IBooksService
    {
        Task<IEnumerable<BooksAllViewModel>> GetAllBooksOrderedByTitleThenByGenreAscAsync(Guid? userId);

        Task<IEnumerable<BooksAllViewModel>> GetBooksCreatedByUserOrderedByTitleThenByGenreAscAsync(Guid userId);

        Task<BookDetailsViewModel> GetBookDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync();

        Task<bool> IsBookAddedByUserAsync(Guid? userId, Guid bookId);

        Task<bool> IsBookAddedToUserCollectionAsync(Guid? userId, Guid bookId);

        Task<BookCreateViewModel> GetBookCreateViewModelAsync();

        Task CreateBookAsync(BookCreateViewModel model, Guid userId);
        Task<IEnumerable<BookFavoritesViewModel>> GetFavoriteBooksAsync(Guid userId);
        Task SaveFevBookAsync(Guid id, Guid userId);

        Task RemoveFevBookAsync(Guid id, Guid userId);

        Task<BookEditViewModel> GetBookForEditAsync(Guid id, Guid userId);
        Task EditBookAsync(BookEditViewModel model, Guid userId);

        Task<BookDeleteViewModel>GetBookDeleteDetailsAsync(Guid id, Guid userId);

        Task DeleteBookAsync(Guid id, Guid userId);
    }
}
