using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Models.Book;

namespace OnlineLibrary.Services.Core.Interfaces
{
    public interface IBooksService
    {
        Task<(IEnumerable<BookAllDto> BooksAllDtos, int TotalPages)> GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(Guid? userId, string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1, int pageSize = 5);
        Task<(IEnumerable<BookAllDto> BooksAllDtos, int TotalPages)> GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(Guid userId, string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1, int pageSize = 5);

        Task<BookDetailsDto?> GetBookDtoDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAllAuthorsAndPublishersAsync();

        Task<bool> IsBookDtoAddedByUserAsync(Guid? userId, Guid bookId);

        Task<bool> IsBookDtoAddedToUserCollectionAsync(Guid? userId, Guid bookId);

        Task<BookCreateDto> GetBookDtoCreateViewModelAsync();

        Task CreateDtoBookAsync(BookCreateDto model, Guid userId);
        Task<IEnumerable<BookFavoritesDto>> GetFavoriteBooksDtoAsync(Guid userId, string? searchQuery = null);
        Task SaveFevBookDtoAsync(Guid id, Guid userId);

        Task RemoveFevBookDtoAsync(Guid id, Guid userId);

        Task<BookEditDto?> GetBookForEditDtoAsync(Guid id, Guid userId);
        Task<bool> EditBookDtoAsync(BookEditDto model, Guid userId);

        Task<BookDeleteDto?> GetBookDeleteDetailsDtoAsync(Guid id, Guid userId);

        Task<bool> DeleteBookDtoAsync(Guid id, Guid userId);
    }
}
