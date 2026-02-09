using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Books;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace OnlineLibrary.Services.Core
{
    public class BooksService : IBooksService
    {
        private readonly OnlineLibraryDbContext dbContext;
        public BooksService(OnlineLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public async Task<IEnumerable<BooksAllViewModel>> GetAllBooksOrderedByTitleThenByGenreAscAsync(string? userId)
        {
            var allBooks = await dbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.UsersBooks)
                .Include(b => b.Publisher)
                .Include(b => b.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.AddedByUser) // ensure username is loaded
                .AsNoTracking()
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
                .Select(b => new BooksAllViewModel
                {
                    Id = b.Id,
                    Title = b.Title,
                    Genre = b.Genre,
                    GenreName = b.Genre.ToString(),
                    Rating = b.Rating,
                    CoverUrl = b.CoverUrl,
                    AddedByUserName = b.AddedByUser.UserName, // null-safe
                    PublisherId = b.PublisherId,
                    PublisherName = b.PublisherName,
                    IsAddedByUser = userId != null && b.AddedByUser != null && b.AddedByUser.Id == userId,
                    IsAddedToUserCollection = userId != null && b.UsersBooks.Any(ub => ub.UserId == userId && ub.BookId == b.Id)
                })
                .ToListAsync();

            return allBooks;
        }

        // Return raw Publisher/Author lists. Controller creates SelectList and assigns to ViewBag.
        public async Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync()
        {
            var publishers = await dbContext.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync();

            var authors = await dbContext.Authors
                .OrderBy(a => a.FullName)
                .ToListAsync();

            return (publishers, authors);
        }


        public async Task<BookDetailsViewModel> GetBookDetailsByIdAsync(Guid id)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await dbContext
                .Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Publisher)
                .Include(b => b.UsersBooks)
                .Include(b => b.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.AddedByUser) // <-- ensure AddedByUser is loaded
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookEntity == null)
            {
                throw new InvalidOperationException("Destination not found");
            }

            // Map to view model in-memory (safe for string.Join and enum ToString)
            var bookDetails = new BookDetailsViewModel
            {
                Id = bookEntity.Id,
                Title = bookEntity.Title,
                Description = bookEntity.Description,
                Genre = bookEntity.Genre,
                GenreName = bookEntity.Genre.ToString(),
                isRead = bookEntity.isRead,
                DateRead = bookEntity.DateRead,
                Rating = bookEntity.Rating,
                CoverUrl = bookEntity.CoverUrl,
                DateAdded = bookEntity.DateAdded,
                PublisherId = bookEntity.PublisherId,
                PublisherName = bookEntity.Publisher?.Name ?? string.Empty,
                AuthorsName = string.Join(", ", bookEntity.BooksAuthors
                    .Select(ba => ba.Author.FullName)),
                AddedByUserName = bookEntity.AddedByUser?.UserName ?? string.Empty, // safe access
                IsAddedByUser = false,
                IsAddedToUserCollection = false
            };
            return bookDetails;
        }

        public async Task<bool> IsBookAddedByUserAsync(string? userId, Guid bookId)
        {
            return await dbContext.Books
                 .AnyAsync(b => b.AddedByUserId == userId && b.Id == bookId);
        }

        public async Task<bool> IsBookAddedToUserCollectionAsync(string? userId, Guid bookId)
        {
            return await dbContext.UsersBooks
                .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId && userId != null);
        }



        public async Task<BookCreateViewModel> GetBookCreateViewModelAsync()
        {
            await GetAuthorsAndPublishersAsync();

            BookCreateViewModel createModel = new BookCreateViewModel();

            return createModel;
        }

        public async Task CreateBookAsync(BookCreateViewModel inputModel, string? userId)
        {

            var book = new Book
            {
                Title = inputModel.Title,
                Description = inputModel.Description,
                Genre = Enum.Parse<BookGenre>(inputModel.Genre),
                isRead = inputModel.isRead,
                DateRead = inputModel.DateRead,
                Rating = inputModel.Rating,
                CoverUrl = inputModel.CoverUrl,
                DateAdded = inputModel.DateAdded,
                PublisherId = inputModel.PublisherId,
                AddedByUserId = userId,

                IsDeleted = false
            };

            try
            {
                // Save book first so the DB generates Id
                await dbContext.Books.AddAsync(book);
                await dbContext.SaveChangesAsync(); // book.Id populated
            }
            catch (Exception)
            {

                throw new InvalidOperationException("\"An error occurred while saving the book. Please try again.\"");
            }

            // Create BookAuthor records linking saved book to selected authors
            if (inputModel.AuthorIds != null && inputModel.AuthorIds.Any())
            {
                var bookAuthors = inputModel.AuthorIds
                       .Select(autorId => new BookAuthor
                       {
                           AuthorId = autorId,
                           BookId = book.Id
                       })
                       .ToList();
                try
                {
                    await dbContext.BooksAuthors.AddRangeAsync(bookAuthors);
                    await dbContext.SaveChangesAsync();
                }
                catch (Exception)
                {
                    throw new InvalidOperationException("At least one author must be selected.");
                }

            }
        }

        public async Task<IEnumerable<BookFavoritesViewModel>> GetFavoriteBooksAsync(string userId)
        {
            var fevBooks = await dbContext.UsersBooks
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Book)
                .Select(ub => new BookFavoritesViewModel
                {
                    Id = ub.Book.Id,
                    Title = ub.Book.Title,
                    CoverUrl = ub.Book.CoverUrl
                })
                .ToListAsync();
            return fevBooks;
        }

        public async Task SaveFevBookAsync(Guid id, string userId)
        {
            if (await dbContext.UsersBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == id))
            {
                return;
            }

            var userBook = new UserBook
            {
                BookId = id,
                UserId = userId
            };

            await dbContext.UsersBooks.AddAsync(userBook);
            await dbContext.SaveChangesAsync();
        }

        public async Task RemoveFevBookAsync(Guid id, string userId)
        {
            var userBook = await dbContext.UsersBooks
                 .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == id);

            if (userBook == null)
            {
                return;
            }

            dbContext.UsersBooks.Remove(userBook);
            await dbContext.SaveChangesAsync();
        }

        public async Task<BookEditViewModel> GetBookForEditAsync(Guid id, string userId)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await dbContext
                .Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.Publisher)
                .Include(b => b.UsersBooks)
                .Include(b => b.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.AddedByUser) // <-- ensure AddedByUser is loaded
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (bookEntity == null)
            {
                throw new InvalidOperationException("Destination not found");
            }

            // Map to view model in-memory (safe for string.Join and enum ToString)
            var bookDetails = new BookEditViewModel
            {
                Id = bookEntity.Id,
                Title = bookEntity.Title,
                Description = bookEntity.Description,
                Genre = bookEntity.Genre.ToString(),
                isRead = bookEntity.isRead,
                DateRead = bookEntity.DateRead,
                Rating = bookEntity.Rating,
                CoverUrl = bookEntity.CoverUrl,
                DateAdded = bookEntity.DateAdded,
                PublisherId = bookEntity.PublisherId,
                AuthorIds = bookEntity.BooksAuthors.Select(ba => ba.AuthorId).ToList()
            };

            return (bookDetails);
        }

        public async Task EditBookAsync(BookEditViewModel inputModel, string userId)
        {
            var bookEntity = dbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.BooksAuthors)
                .FirstOrDefault(b => b.Id == inputModel.Id);

            if (bookEntity == null)
            {
                throw new InvalidOperationException("Book not found");
            }

            try
            {
                // Update book properties
                bookEntity.Title = inputModel.Title;
                bookEntity.Description = inputModel.Description;
                bookEntity.Genre = Enum.Parse<BookGenre>(inputModel.Genre);
                bookEntity.isRead = inputModel.isRead;
                bookEntity.DateRead = inputModel.DateRead;
                bookEntity.Rating = inputModel.Rating;
                bookEntity.CoverUrl = inputModel.CoverUrl;
                bookEntity.DateAdded = inputModel.DateAdded;
                bookEntity.PublisherId = inputModel.PublisherId;
                // Update BookAuthor relationships
                var existingAuthorIds = bookEntity.BooksAuthors
                    .Select(ba => ba.AuthorId)
                    .ToList();

                var newAuthorIds = inputModel.AuthorIds ?? new List<int>();

                // Remove unselected authors
                var toRemove = bookEntity.BooksAuthors
                    .Where(ba => !newAuthorIds.Contains(ba.AuthorId))
                    .ToList();

                dbContext.BooksAuthors.RemoveRange(toRemove);

                // Add newly selected authors
                var toAdd = newAuthorIds
                    .Except(existingAuthorIds)
                    .Select(authorId => new BookAuthor
                    {
                        BookId = bookEntity.Id,
                        AuthorId = authorId
                    });

                await dbContext.BooksAuthors.AddRangeAsync(toAdd);
                await dbContext.SaveChangesAsync();

                          }
            catch (Exception)
            {
                throw new InvalidOperationException("An error occurred while updating the book. Please try again.");

            }



        }

        public async Task<BookDeleteViewModel> GetBookDeleteDetailsAsync(Guid id, string userId)
        {
            var book = await dbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.AddedByUser)
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                throw new ArgumentException("Book not found");
            }

            if (book.AddedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this book.");
            }

            var deleteModel = new BookDeleteViewModel
            {
                Id = book.Id,
                Title = book.Title,
                AddedByUserName = book.AddedByUser?.UserName, // null-safe access
                CoverUrl = book.CoverUrl
            };

            return deleteModel;

        }

        public async Task DeleteBookAsync(Guid id, string userId)
        {
            var book = await dbContext.Books
                 .FirstOrDefaultAsync(b => b.Id == id && !b.IsDeleted);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            if (book.AddedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this book.");
            }

            book.IsDeleted = true;
            await dbContext.SaveChangesAsync();
        }
    }
}

