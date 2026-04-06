using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Repository.Contracts
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksOrderedByTitleThenByGenreAscAsync(Guid? userId);

        Task<IEnumerable<Book>> GetBooksByUserOrderedByTitleThenByGenreAscAsync(Guid userId);

        Task<Book?> GetBookDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync();

        Task<bool> IsBookAddedByUserAsync(Guid? userId, Guid bookId);

        Task<bool> IsBookAddedToUserCollectionAsync(Guid? userId, Guid bookId);

        Task<Book> GetBookCreateViewModelAsync();

        Task CreateBookAsync(Book model, Guid userId);

        Task<IEnumerable<UserBook>> GetFavoriteBooksAsync(Guid userId);

        Task SaveFevBookAsync(Guid id, Guid userId);

        Task RemoveFevBookAsync(Guid id, Guid userId);

        Task UpdateFavBookReadStatusAsync(Guid userId, Guid bookId, bool isRead, DateTime? dateRead);

        Task<Book?> GetBookForEditAsync(Guid id, Guid userId);
        Task<bool> EditBookAsync(Book model, Guid userId);

        Task<Book?> GetBookDeleteDetailsAsync(Guid id, Guid userId);

        Task<bool> DeleteBookAsync(Guid id, Guid userId);

        // Admin-specific: bypass ownership checks
        Task<Book?> GetBookForAdminEditAsync(Guid id);
        Task<bool> EditBookForAdminAsync(Book model);

        Task<Book?> GetBookAdminDeleteDetailsAsync(Guid id);
        Task<bool> DeleteBookForAdminAsync(Guid id);

        Task<IEnumerable<Book>> GetAllBooksForAdminAsync();
        Task<bool> RestoreBookForAdminAsync(Guid id);
    }
}
