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



        public async Task<(IEnumerable<BookAllDto> BooksAllDtos, int TotalPages)> GetAllBooksDtoOrderedByTitleThenByGenreAscAsync(Guid? userId, string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1, int pageSize = 5)
        {
            var allBooks = await bookRepository.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Title.ToLower().Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(publisherFilter))
            {
                publisherFilter = publisherFilter.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Publisher.Name.ToLower().Contains(publisherFilter));
            }

            if (!string.IsNullOrEmpty(genreFilter))
            {
                genreFilter = genreFilter.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Genre.ToString().ToLower().Contains(genreFilter));
            }

            var orderedBooks = allBooks
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Genre);

            int totalBooks = orderedBooks.Count();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var allBooksDto = orderedBooks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookAllDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    GenreName = b.Genre.ToString(),
                    Rating = b.Rating,
                    CoverUrl = b.CoverUrl ?? string.Empty,
                    AddedByUserName = b.AddedByUser.UserName,
                    PublisherId = b.PublisherId,
                    PublisherName = b.Publisher.Name,
                    IsAddedByUser = userId != null && b.AddedByUser != null && b.AddedByUser.Id == userId,
                    IsAddedToUserCollection = userId != null && b.UsersBooks.Any(ub => ub.UserId == userId && ub.BookId == b.Id)
                })
                .ToList();

            return (allBooksDto, totalPages);
        }

        public async Task<(IEnumerable<BookAllDto> BooksAllDtos, int TotalPages)> GetBooksDtoCreatedByUserOrderedByTitleThenByGenreAscAsync(Guid userId, string? searchQuery = null, string? publisherFilter = null, string? genreFilter = null, int pageNumber = 1, int pageSize = 5)
        {
            // Use the dedicated repository method that filters at DB level
            var allBooks = await bookRepository.GetBooksByUserOrderedByTitleThenByGenreAscAsync(userId);


            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Title.ToLower().Contains(searchQuery));
            }

            if (!string.IsNullOrEmpty(publisherFilter))
            {
                publisherFilter = publisherFilter.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Publisher.Name.ToLower().Contains(publisherFilter));
            }

            if (!string.IsNullOrEmpty(genreFilter))
            {
                genreFilter = genreFilter.Trim().ToLower();
                allBooks = allBooks.Where(b => b.Genre.ToString().ToLower().Contains(genreFilter));
            }

            var orderedBooks = allBooks
                .OrderBy(b => b.Title)
                .ThenBy(b => b.Genre);

            int totalBooks = orderedBooks.Count();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var allBooksDto = orderedBooks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(b => new BookAllDto
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    GenreName = b.Genre.ToString(),
                    Rating = b.Rating,
                    CoverUrl = b.CoverUrl ?? string.Empty,
                    AddedByUserName = b.AddedByUser.UserName,
                    PublisherId = b.PublisherId,
                    PublisherName = b.Publisher.Name,
                    IsAddedByUser = b.AddedByUserId == userId,
                    IsAddedToUserCollection = b.UsersBooks.Any(ub => ub.UserId == userId && ub.BookId == b.Id)
                })
                .ToList();

            return (allBooksDto, totalPages);
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
                Rating = bookEntity.Rating,
                CoverUrl = bookEntity.CoverUrl ?? string.Empty,
                DateAdded = bookEntity.DateAdded.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                PublisherId = bookEntity.PublisherId,
                PublisherName = bookEntity.Publisher?.Name ?? string.Empty,
                AuthorsName = string.Join(", ", bookEntity.BooksAuthors
                    .Where(ba => !ba.IsDeleted)
                    .Select(ba => ba.Author.FullName)),
                AddedByUserName = bookEntity.AddedByUser?.UserName ?? string.Empty,
                IsAddedByUser = false,
                IsAddedToUserCollection = false
            };

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
                Rating = inputModel.Rating,
                CoverUrl = inputModel.CoverUrl ?? string.Empty,
                DateAdded = inputModel.DateAdded,
                PublisherId = inputModel.PublisherId,
                AddedByUserId = userId,
                IsDeleted = false
            };

            await bookRepository.CreateBookAsync(bookInputModel, userId);
        }

        public async Task<(IEnumerable<BookFavoritesDto> BookFavoritesDtos, int TotalPages)> GetFavoriteBooksDtoAsync(Guid userId, string? searchQuery = null, int pageNumber = 1, int pageSize = 5)
        {
            var favUserBooks = await bookRepository.GetFavoriteBooksAsync(userId);

            if (!string.IsNullOrEmpty(searchQuery))
            {
                searchQuery = searchQuery.Trim().ToLower();
                favUserBooks = favUserBooks.Where(ub => ub.Book.Title.ToLower().Contains(searchQuery));
            }

            var orderedFavBooks = favUserBooks
                .OrderBy(ub => ub.Book.Title);

            int totalBooks = orderedFavBooks.Count();
            int totalPages = (int)Math.Ceiling(totalBooks / (double)pageSize);

            var favBooksDto = orderedFavBooks
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .Select(MapUserBookToBookFavoritesDto)
                .ToList();

            return (favBooksDto, totalPages);
        }

        public async Task SaveFevBookDtoAsync(Guid id, Guid userId)
        {
            await bookRepository.SaveFevBookAsync(id, userId);
        }

        public async Task RemoveFevBookDtoAsync(Guid id, Guid userId)
        {
            await bookRepository.RemoveFevBookAsync(id, userId);
        }

        public async Task UpdateFavBookReadStatusDtoAsync(Guid id, Guid userId, bool isRead, DateTime? dateRead)
        {
            await bookRepository.UpdateFavBookReadStatusAsync(userId, id, isRead, dateRead);
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
    }
}
