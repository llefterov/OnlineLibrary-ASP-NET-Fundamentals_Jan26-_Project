using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using OnlineLibrary.Services.Models.Book;
using static OnlineLibrary.Services.CustomMappers.BookMappers;

namespace OnlineLibrary.Services.Core.Admin
{
    public class BookManagementService : BooksService, IBookManagementService
    {
        private readonly IBookRepository bookRepository;

        public BookManagementService(IBookRepository bookRepository)
            : base(bookRepository)
        {
            this.bookRepository = bookRepository;
        }

        public async Task<BookEditDto?> GetBookForAdminEditDtoAsync(Guid id)
        {
            var bookEntity = await bookRepository.GetBookForAdminEditAsync(id);

            if (bookEntity == null)
            {
                return null;
            }

            return MapBookToBookEditDto(bookEntity);
        }

        public async Task<bool> EditBookForAdminDtoAsync(BookEditDto model)
        {
            var book = MapBookEditDtoToBook(model);
            return await bookRepository.EditBookForAdminAsync(book);
        }

        public async Task<BookDeleteDto?> GetBookAdminDeleteDetailsDtoAsync(Guid id)
        {
            var book = await bookRepository.GetBookAdminDeleteDetailsAsync(id);

            if (book == null)
            {
                return null;
            }

            return MapBookToBookDeleteDto(book);
        }

        public async Task<bool> DeleteBookForAdminDtoAsync(Guid id)
        {
            return await bookRepository.DeleteBookForAdminAsync(id);
        }

        public async Task<IEnumerable<BookAllDto>> GetAllBooksForAdminDtoAsync()
        {
            var books = await bookRepository.GetAllBooksForAdminAsync();

            return books.Select(b => new BookAllDto
            {
                Id = b.Id,
                Title = b.Title,
                Genre = b.Genre,
                GenreName = b.Genre.ToString(),
                Rating = b.Rating,
                CoverUrl = b.CoverUrl ?? string.Empty,
                AddedByUserName = b.AddedByUser?.UserName,
                PublisherId = b.PublisherId,
                PublisherName = b.Publisher.Name,
                IsDeleted = b.IsDeleted
            }).ToList();
        }

        public async Task<bool> RestoreBookForAdminDtoAsync(Guid id)
        {
            return await bookRepository.RestoreBookForAdminAsync(id);
        }
    }
}
