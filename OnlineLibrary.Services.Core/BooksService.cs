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

             // Save book first so the DB generates Id
                dbContext.Books.Add(book);
               await dbContext.SaveChangesAsync(); // book.Id populated
          
            
        }
    }
}
