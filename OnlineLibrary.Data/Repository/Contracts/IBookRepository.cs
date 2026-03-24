using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data.Repository.Contracts
{
    public interface IBookRepository
    {
        Task<IEnumerable<Book>> GetAllBooksOrderedByTitleThenByGenreAscAsync(Guid? userId);

        Task<Book> GetBookDetailsByIdAsync(Guid id);

        // Return raw model lists for Authors and Publishers; the controller constructs SelectList/ViewBag.
        Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync();

        Task<bool> IsBookAddedByUserAsync(Guid? userId, Guid bookId);

        Task<bool> IsBookAddedToUserCollectionAsync(Guid? userId, Guid bookId);

        Task<Book> GetBookCreateViewModelAsync();

        Task CreateBookAsync(Book model, Guid userId);

        //Task<IEnumerable<BookFavoritesViewModel>> GetFavoriteBooksAsync(Guid userId);
        //Task SaveFevBookAsync(Guid id, Guid userId);

        //Task RemoveFevBookAsync(Guid id, Guid userId);

        //Task<BookEditViewModel> GetBookForEditAsync(Guid id, Guid userId);
        //Task EditBookAsync(BookEditViewModel model, Guid userId);

        //Task<BookDeleteViewModel> GetBookDeleteDetailsAsync(Guid id, Guid userId);

        //Task DeleteBookAsync(Guid id, Guid userId);


    }
}
