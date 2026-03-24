using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.ViewModels.Books;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IBooksService
    {
        Task<IEnumerable<BookAllDto>> GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(Guid? userId);
        Task<IEnumerable<BookAllDto>> GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(Guid userId);

        Task<BookDetailsDto> GetBookDtoDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAllAuthorsAndPublishersAsync();

        Task<bool> IsBookDtoAddedByUserAsync(Guid? userId, Guid bookId);

        Task<bool> IsBookDtoAddedToUserCollectionAsync(Guid? userId, Guid bookId);

        //Task<BookCreateViewModel> GetBookCreateViewModelAsync();

        //Task CreateBookAsync(BookCreateViewModel model, Guid userId);
        //Task<IEnumerable<BookFavoritesViewModel>> GetFavoriteBooksAsync(Guid userId);
        //Task SaveFevBookAsync(Guid id, Guid userId);

        //Task RemoveFevBookAsync(Guid id, Guid userId);

        //Task<BookEditViewModel> GetBookForEditAsync(Guid id, Guid userId);
        //Task EditBookAsync(BookEditViewModel model, Guid userId);

        //Task<BookDeleteViewModel>GetBookDeleteDetailsAsync(Guid id, Guid userId);

        //Task DeleteBookAsync(Guid id, Guid userId);
    }
}
