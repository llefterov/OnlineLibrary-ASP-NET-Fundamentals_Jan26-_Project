using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Book;
using OnlineLibrary.Web.ViewModels.Books;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using static OnlineLibrary.GCommon.ApplicationConstants;

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


        public async Task<BookDetailsDto> GetBookDtoDetailsByIdAsync(Guid id)
        {
            // Load the entity with related data first (server-side)
         var bookEntity = await bookRepository.GetBookDetailsByIdAsync(id);

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

        //public async Task<IEnumerable<BookFavoritesViewModel>> GetFavoriteBooksAsync(Guid userId)
        //{
        //    var fevBooks = await dbContext.UsersBooks
        //        .Where(ub => ub.UserId == userId)
        //        .Include(ub => ub.Book)
        //        .Select(ub => new BookFavoritesViewModel
        //        {
        //            Id = ub.Book.Id,
        //            Title = ub.Book.Title,
        //            CoverUrl = ub.Book.CoverUrl ?? string.Empty
        //        })
        //        .ToListAsync();
        //    return fevBooks;
        //}

        //public async Task SaveFevBookAsync(Guid id, Guid userId)
        //{
        //    if (await dbContext.UsersBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == id))
        //    {
        //        return;
        //    }

        //    var userBook = new UserBook
        //    {
        //        BookId = id,
        //        UserId = userId
        //    };

        //    await dbContext.UsersBooks.AddAsync(userBook);
        //    await dbContext.SaveChangesAsync();
        //}

        //public async Task RemoveFevBookAsync(Guid id, Guid userId)
        //{
        //    var userBook = await dbContext.UsersBooks
        //         .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == id);

        //    if (userBook == null)
        //    {
        //        return;
        //    }

        //    dbContext.UsersBooks.Remove(userBook);
        //    await dbContext.SaveChangesAsync();
        //}

        //public async Task<BookEditViewModel> GetBookForEditAsync(Guid id, Guid userId)
        //{
        //    // Load the entity with related data first (server-side)
        //    var bookEntity = await dbContext
        //        .Books
        //        .Where(b => !b.IsDeleted)
        //        .Include(b => b.Publisher)
        //        .Include(b => b.UsersBooks)
        //        .Include(b => b.BooksAuthors)
        //            .ThenInclude(ba => ba.Author)
        //        .Include(b => b.AddedByUser) // <-- ensure AddedByUser is loaded
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(b => b.Id == id);

        //    if (bookEntity == null)
        //    {
        //        throw new InvalidOperationException("Destination not found");
        //    }

        //    // Map to view model in-memory (safe for string.Join and enum ToString)
        //    var bookDetails = new BookEditViewModel
        //    {
        //        Id = bookEntity.Id,
        //        Title = bookEntity.Title,
        //        Description = bookEntity.Description,
        //        Genre = bookEntity.Genre,
        //        IsRead = bookEntity.IsRead,
        //        DateRead = bookEntity.DateRead,
        //        Rating = bookEntity.Rating,
        //        CoverUrl = bookEntity.CoverUrl,
        //        DateAdded = bookEntity.DateAdded,
        //        PublisherId = bookEntity.PublisherId,
        //        AuthorIds = bookEntity.BooksAuthors.Select(ba => ba.AuthorId).ToList()
        //    };

        //    return (bookDetails);
        //}

        //public async Task EditBookAsync(BookEditViewModel inputModel, Guid userId)
        //{
        //    var bookEntity = dbContext.Books
        //        .Where(b => !b.IsDeleted)
        //        .Include(b => b.BooksAuthors)
        //        .FirstOrDefault(b => b.Id == inputModel.Id);

        //    if (bookEntity == null)
        //    {
        //        throw new InvalidOperationException("Book not found");
        //    }

        //    // Update book properties
        //    bookEntity.Title = inputModel.Title;
        //    bookEntity.Description = inputModel.Description;
        //    bookEntity.Genre = inputModel.Genre;
        //    bookEntity.IsRead = inputModel.IsRead;
        //    bookEntity.DateRead = inputModel.DateRead;
        //    bookEntity.Rating = inputModel.Rating;
        //    bookEntity.CoverUrl = inputModel.CoverUrl ?? string.Empty;
        //    bookEntity.DateAdded = inputModel.DateAdded;
        //    bookEntity.PublisherId = inputModel.PublisherId;
        //    // Update BookAuthor relationships
        //    var existingAuthorIds = bookEntity.BooksAuthors
        //        .Select(ba => ba.AuthorId)
        //        .ToList();

        //    // Validate publisher exists
        //    if (!await dbContext.Publishers.AnyAsync(p => p.Id == inputModel.PublisherId))
        //    {
        //        throw new PublisherDoesntExistException("Selected publisher does not exist.");
        //    }

        //    // Validate provided author ids (if any) before creating the book to avoid FK errors
        //    if (inputModel.AuthorIds != null && inputModel.AuthorIds.Any())
        //    {
        //        var validAuthorIds = await dbContext.Authors
        //            .Where(a => inputModel.AuthorIds.Contains(a.Id))
        //            .Select(a => a.Id)
        //            .ToListAsync();

        //        var invalidIds = inputModel.AuthorIds.Except(validAuthorIds).ToList();
        //        if (invalidIds.Any())
        //        {
        //            throw new AuthorDoesntExistException("One or more selected authors are invalid.");
        //        }
        //    }

        //    try
        //    {
        //        var newAuthorIds = inputModel.AuthorIds ?? new List<Guid>();

        //        // Remove unselected authors
        //        var toRemove = bookEntity.BooksAuthors
        //            .Where(ba => !newAuthorIds.Contains(ba.AuthorId))
        //            .ToList();

        //        dbContext.BooksAuthors.RemoveRange(toRemove);

        //        // Add newly selected authors
        //        var toAdd = newAuthorIds
        //            .Except(existingAuthorIds)
        //            .Select(authorId => new BookAuthor
        //            {
        //                BookId = bookEntity.Id,
        //                AuthorId = authorId
        //            });

        //        await dbContext.BooksAuthors.AddRangeAsync(toAdd);
        //        await dbContext.SaveChangesAsync();

        //    }
        //    catch (Exception)
        //    {
        //        throw new InvalidOperationException("An error occurred while updating the book. Please try again.");
        //    }
        //}

        //public async Task<BookDeleteViewModel> GetBookDeleteDetailsAsync(Guid id, Guid userId)
        //{
        //    var book = await dbContext.Books
        //        .Where(b => !b.IsDeleted)
        //        .Include(b => b.AddedByUser)
        //        .Include(ba => ba.BooksAuthors)
        //            .ThenInclude(ba => ba.Author)
        //        .AsNoTracking()
        //        .FirstOrDefaultAsync(b => b.Id == id);

        //    if (book == null)
        //    {
        //        throw new ArgumentException("Book not found");
        //    }

        //    if (book.AddedByUserId != userId)
        //    {
        //        throw new UnauthorizedAccessException("You are not authorized to delete this book.");
        //    }

        //    var deleteModel = new BookDeleteViewModel
        //    {
        //        Id = book.Id,
        //        Title = book.Title,
        //        AddedByUserName = book.AddedByUser?.UserName, // null-safe access
        //        CoverUrl = book.CoverUrl
        //    };

        //    return deleteModel;
        //}

        //public async Task DeleteBookAsync(Guid id, Guid userId)
        //{
        //    // Load tracked entity with related collections (no AsNoTracking)
        //    var book = await dbContext.Books
        //        .Where(b => !b.IsDeleted)
        //        .Include(b => b.AddedByUser)
        //        .FirstOrDefaultAsync(b => b.Id == id);

        //    if (book == null)
        //    {
        //        throw new ArgumentException("Book not found.");
        //    }

        //    if (book.AddedByUserId != userId)
        //    {
        //        throw new UnauthorizedAccessException("You are not authorized to delete this book.");
        //    }

        //    // Remove dependent BookAuthor entries
        //    var bookAuthorEntries = dbContext.BooksAuthors
        //        .Where(ba => ba.BookId == id);
        //    dbContext.BooksAuthors.RemoveRange(bookAuthorEntries);

        //    // Remove dependent UserBook entries (user collections)
        //    var userBookEntries = dbContext.UsersBooks
        //        .Where(ub => ub.BookId == id);
        //    dbContext.UsersBooks.RemoveRange(userBookEntries);

        //    // Soft-delete the book
        //    book.IsDeleted = true;

        //    await dbContext.SaveChangesAsync();
        //}
    }
}

