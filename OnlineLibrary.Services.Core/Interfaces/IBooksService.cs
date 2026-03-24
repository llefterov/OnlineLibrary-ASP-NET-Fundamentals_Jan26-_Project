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

        Task<BookCreateDto> GetBookDtoCreateViewModelAsync();

        Task CreateDtoBookAsync(BookCreateDto model, Guid userId);
        Task<IEnumerable<BookFavoritesDto>> GetFavoriteBooksDtoAsync(Guid userId);
        Task SaveFevBookDtoAsync(Guid id, Guid userId);

        Task RemoveFevBookDtoAsync(Guid id, Guid userId);

        Task<BookEditDto> GetBookForEditDtoAsync(Guid id, Guid userId);
        Task EditBookDtoAsync(BookEditDto model, Guid userId);

        Task<BookDeleteDto>GetBookDeleteDetailsDtoAsync(Guid id, Guid userId);

        Task DeleteBookDtoAsync(Guid id, Guid userId);
    }
}
