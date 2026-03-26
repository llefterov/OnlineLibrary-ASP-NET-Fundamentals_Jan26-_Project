using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Book;
using System.Globalization;
using static OnlineLibrary.GCommon.ApplicationConstants;
using static OnlineLibrary.Services.CustomMappers.BookMappers;

namespace OnlineLibrary.Services.Core
{
    public class BooksService : IBooksService
    {
        private readonly IBookRepository bookRepository;
        public BooksService(IBookRepository bookRepository)
        {
            this.bookRepository = bookRepository;
        }



        public async Task<IEnumerable<BookAllDto>> GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(Guid? userId)
        {
            var allBooks = await bookRepository.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId);

            var allBooksDto = allBooks
                .Select(b => new
                {
                    b.Id,
                    b.Title,
                    b.Description,
                    b.Genre,
                    b.CoverUrl,
                    b.AddedByUser,
                    b.PublisherId,
                    b.Rating,
                    PublisherName = b.Publisher.Name,
                    b.UsersBooks
                })
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Genre)
                .Select(b => new BookAllDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    GenreName = b.Genre.ToString(),
                    Rating = b.Rating,
                    CoverUrl = b.CoverUrl ?? string.Empty,
                    AddedByUserName = b.AddedByUser.UserName, // null-safe
                    PublisherId = b.PublisherId,
                    PublisherName = b.PublisherName,
                    IsAddedByUser = userId != null && b.AddedByUser != null && b.AddedByUser.Id == userId,
                    IsAddedToUserCollection = userId != null && b.UsersBooks.Any(ub => ub.UserId == userId && ub.BookId == b.Id)
                })
                .ToList();

            return allBooksDto;
        }

        public async Task<IEnumerable<BookAllDto>> GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(Guid userId)
        {
            var allBooks = await bookRepository
                    .GetAllBooksOrderedByTitleThenByGenreAscAsync(userId);

            var allBooksDto = allBooks
               .Select(b => new
               {
                   b.Id,
                   b.Title,
                   b.Description,
                   b.Genre,
                   b.CoverUrl,
                   b.AddedByUser,
                   b.PublisherId,
                   b.Rating,
                   PublisherName = b.Publisher.Name,
                   b.UsersBooks
               })
               .OrderBy(b => b.Title)
               .ThenBy(b => b.Genre)
               .Select(b => new BookAllDto
               {
                   Id = b.Id,
                   Title = b.Title,
                   Genre = b.Genre,
                   GenreName = b.Genre.ToString(),
                   Rating = b.Rating,
                   CoverUrl = b.CoverUrl ?? string.Empty,
                   AddedByUserName = b.AddedByUser.UserName, // null-safe
                   PublisherId = b.PublisherId,
                   PublisherName = b.PublisherName,
                   IsAddedByUser = userId != Guid.Empty && b.AddedByUser != null && b.AddedByUser.Id == userId,
                   IsAddedToUserCollection = userId != Guid.Empty && b.UsersBooks.Any(ub => ub.UserId == userId && ub.BookId == b.Id)
               })
               .Where(b => b.IsAddedByUser == true)
               .ToList();

            return allBooksDto;
        }

        // Return raw Publisher/Author lists. Controller creates SelectList and assigns to ViewBag.
        public async Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAllAuthorsAndPublishersAsync()
        {
            var (publishers, authors) = await bookRepository.GetAuthorsAndPublishersAsync();

            return (publishers, authors);
        }


        public async Task<BookDetailsDto?> GetBookDtoDetailsByIdAsync(Guid id)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await bookRepository.GetBookDetailsByIdAsync(id);

            if (bookEntity == null)
            {
                return null;
            }

            // Map to view model in-memory (safe for string.Join and enum ToString)
            var bookDetailsDto = new BookDetailsDto
            {
                Id = bookEntity.Id,
                Title = bookEntity.Title,
                Description = bookEntity.Description,
                Genre = bookEntity.Genre,
                GenreName = bookEntity.Genre.ToString(),
                IsRead = bookEntity.IsRead,
                DateRead = bookEntity.DateRead?.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                Rating = bookEntity.Rating,
                CoverUrl = bookEntity.CoverUrl ?? string.Empty,
                DateAdded = bookEntity.DateAdded.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                PublisherId = bookEntity.PublisherId,
                PublisherName = bookEntity.Publisher?.Name ?? string.Empty,
                AuthorsName = string.Join(", ", bookEntity.BooksAuthors
                    .Select(ba => ba.Author.FullName)),
                AddedByUserName = bookEntity.AddedByUser?.UserName ?? string.Empty, // safe access
                IsAddedByUser = false,
                IsAddedToUserCollection = false
            };

            if (bookDetailsDto.IsRead == false)
            {
                bookDetailsDto.DateRead = null;
            }

            return bookDetailsDto;
        }

        public async Task<bool> IsBookDtoAddedByUserAsync(Guid? userId, Guid bookId)
        {
            return await bookRepository.IsBookAddedByUserAsync(userId, bookId);
        }
        public async Task<bool> IsBookDtoAddedToUserCollectionAsync(Guid? userId, Guid bookId)
        {
            return await bookRepository.IsBookAddedToUserCollectionAsync(userId, bookId);
        }

        public async Task<BookCreateDto> GetBookDtoCreateViewModelAsync()
        {
            await GetAllAuthorsAndPublishersAsync();

            BookCreateDto createModel = new BookCreateDto();
            return createModel;
        }

        public async Task CreateDtoBookAsync(BookCreateDto inputModel, Guid userId)
        {
            var bookInputModel = new Book
            {
                Title = inputModel.Title,
                Description = inputModel.Description,
                Genre = inputModel.Genre,
                IsRead = inputModel.IsRead,
                DateRead = inputModel.DateRead,
                Rating = inputModel.Rating,
                CoverUrl = inputModel.CoverUrl ?? string.Empty,
                DateAdded = inputModel.DateAdded,
                PublisherId = inputModel.PublisherId,
                AddedByUserId = userId,
                IsDeleted = false
            };

            await bookRepository.CreateBookAsync(bookInputModel, userId);
        }

        public async Task<IEnumerable<BookFavoritesDto>> GetFavoriteBooksDtoAsync(Guid userId)
        {
            var favBooks = await bookRepository.GetFavoriteBooksAsync(userId);
            var favBooksDto = favBooks.Select(MapBookToBookFavoritesDto).ToList();
            return favBooksDto;
        }

        public async Task SaveFevBookDtoAsync(Guid id, Guid userId)
        {
            await bookRepository.SaveFevBookAsync(id, userId);
        }

        public async Task RemoveFevBookDtoAsync(Guid id, Guid userId)
        {
            await bookRepository.RemoveFevBookAsync(id, userId);
        }

        public async Task<BookEditDto?> GetBookForEditDtoAsync(Guid id, Guid userId)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await bookRepository.GetBookForEditAsync(id, userId);

            if (bookEntity == null)
            {
                return null;
            }

            var bookDetails = MapBookToBookEditDto(bookEntity);

            return (bookDetails);
        }

        public async Task<bool> EditBookDtoAsync(BookEditDto inputModel, Guid userId)
        {
            var book = MapBookEditDtoToBook(inputModel);
            return await bookRepository.EditBookAsync(book, userId);
        }

        public async Task<BookDeleteDto?> GetBookDeleteDetailsDtoAsync(Guid id, Guid userId)
        {
            var book = await bookRepository.GetBookDeleteDetailsAsync(id, userId);

            if (book == null)
            {
                return null;
            }

            var bookDeleteDto = MapBookToBookDeleteDto(book);

            return bookDeleteDto;
        }

        public async Task<bool> DeleteBookDtoAsync(Guid id, Guid userId)
        {
            return await bookRepository.DeleteBookAsync(id, userId);
        }

        private static BookFavoritesDto MapBookToBookFavoritesDto(Book book)
        {
            return new BookFavoritesDto
            {
                Id = book.Id,
                Title = book.Title,
                CoverUrl = book.CoverUrl ?? string.Empty
            };
        }
    }
}
