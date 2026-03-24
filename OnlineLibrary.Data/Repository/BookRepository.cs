using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;

namespace OnlineLibrary.Data.Repository
{
    public class BookRepository : BaseRepository, IBookRepository
    {

        public BookRepository(OnlineLibraryDbContext dbContext) : base(dbContext)
        {
            
        }

        public async Task<IEnumerable<Book>> GetAllBooksOrderedByTitleThenByGenreAscAsync(Guid? userId)
        {
            var allBooks = await DbContext
                .Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.UsersBooks)
                .Include(b => b.Publisher)
                .Include(b => b.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
                .Include(b => b.AddedByUser) // ensure username is loaded
                .AsNoTracking()
                .ToListAsync();
           
            return allBooks;
        }


        // Return raw Publisher/Author lists. Controller creates SelectList and assigns to ViewBag.
        public async Task<(IEnumerable<Publisher> Publishers, IEnumerable<Author> Authors)> GetAuthorsAndPublishersAsync()
        {
            var publishers = await DbContext.Publishers
                .OrderBy(p => p.Name)
                .ToListAsync();

            var authors = await DbContext.Authors
                .OrderBy(a => a.FullName)
                .ToListAsync();

            return (publishers, authors);
        }


        public async Task<Book> GetBookDetailsByIdAsync(Guid id)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await DbContext
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
                throw new InvalidOperationException("Book not found");
            }
        
            return bookEntity;
        }

        public async Task<bool> IsBookAddedByUserAsync(Guid? userId, Guid bookId)
        {
            return await DbContext.Books
                 .AnyAsync(b => b.AddedByUserId == userId && b.Id == bookId);
        }
        public async Task<bool> IsBookAddedToUserCollectionAsync(Guid? userId, Guid bookId)
        {
            return await DbContext.UsersBooks
                .AnyAsync(ub => ub.UserId == userId && ub.BookId == bookId && userId != null);
        }

        public async Task<Book> GetBookCreateViewModelAsync()
        {
            await GetAuthorsAndPublishersAsync();

            Book newBook = new Book();

            return newBook;
            ;
        }

        public async Task CreateBookAsync(Book inputModel, Guid userId)
        {
            // Validate publisher exists
            if (!await DbContext.Publishers.AnyAsync(p => p.Id == inputModel.PublisherId))
            {
                throw new PublisherDoesntExistException("Selected publisher does not exist.");
            }

            // Validate provided author ids (if any) before creating the book to avoid FK errors
            if (inputModel.BooksAuthors?.Any() == true)
            {
                var authorIds = inputModel.BooksAuthors.Select(ba => ba.AuthorId).ToList();
                var validAuthorIds = await DbContext.Authors
                    .Where(a => authorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidIds = authorIds.Except(validAuthorIds).ToList();
                if (invalidIds.Any())
                {
                    throw new AuthorDoesntExistException("One or more selected authors are invalid.");
                }
            }

            try
            {
                // Save book first so the DB generates Id
                await DbContext.Books.AddAsync(inputModel);
                await DbContext.SaveChangesAsync(); // inputModel.Id populated
            }
            catch (Exception)
            {
                throw new InvalidOperationException("\"An error occurred while saving the book. Please try again.\"");
            }

            // Create BookAuthor records linking saved book to selected authors
            if (inputModel.BooksAuthors?.Any() == true)
            {
                var bookAuthors = inputModel.BooksAuthors
                       .Select(ba => new BookAuthor
                       {
                           AuthorId = ba.AuthorId,
                           BookId = inputModel.Id
                       })
                       .ToList();

                await DbContext.BooksAuthors.AddRangeAsync(bookAuthors);
                await DbContext.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Book>> GetFavoriteBooksAsync(Guid userId)
        {
            var favBooks = await DbContext.UsersBooks
                .Where(ub => ub.UserId == userId)
                .Include(ub => ub.Book)
                .Select(ub => new Book
                {
                    Id = ub.Book.Id,
                    Title = ub.Book.Title,
                    CoverUrl = ub.Book.CoverUrl ?? string.Empty
                })
                .ToListAsync();
            return favBooks;
        }

        public async Task SaveFevBookAsync(Guid id, Guid userId)
        {
            if (await DbContext.UsersBooks.AnyAsync(ub => ub.UserId == userId && ub.BookId == id))
            {
                return;
            }

            var userBook = new UserBook
            {
                BookId = id,
                UserId = userId
            };

            await DbContext.UsersBooks.AddAsync(userBook);
            await DbContext.SaveChangesAsync();
        }

        public async Task RemoveFevBookAsync(Guid id, Guid userId)
        {
            var userBook = await DbContext.UsersBooks
                 .FirstOrDefaultAsync(ub => ub.UserId == userId && ub.BookId == id);

            if (userBook == null)
            {
                return;
            }

            DbContext.UsersBooks.Remove(userBook);
            await DbContext.SaveChangesAsync();
        }

        public async Task<Book> GetBookForEditAsync(Guid id, Guid userId)
        {
            // Load the entity with related data first (server-side)
            var bookEntity = await DbContext
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
            var bookDetails = new Book
            {
                Id = bookEntity.Id,
                Title = bookEntity.Title,
                Description = bookEntity.Description,
                Genre = bookEntity.Genre,
                IsRead = bookEntity.IsRead,
                DateRead = bookEntity.DateRead,
                Rating = bookEntity.Rating,
                CoverUrl = bookEntity.CoverUrl,
                DateAdded = bookEntity.DateAdded,
                PublisherId = bookEntity.PublisherId,
                AuthorIds = bookEntity.BooksAuthors.Select(ba => ba.AuthorId).ToList()
            };

            return (bookDetails);
        }

        public async Task EditBookAsync(Book inputModel, Guid userId)
        {
            var bookEntity = await DbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.BooksAuthors)
                .FirstOrDefaultAsync(b => b.Id == inputModel.Id);

            if (bookEntity == null)
            {
                throw new InvalidOperationException("Book not found");
            }

            // Update book properties
            bookEntity.Title = inputModel.Title;
            bookEntity.Description = inputModel.Description;
            bookEntity.Genre = inputModel.Genre;
            bookEntity.IsRead = inputModel.IsRead;
            bookEntity.DateRead = inputModel.DateRead;
            bookEntity.Rating = inputModel.Rating;
            bookEntity.CoverUrl = inputModel.CoverUrl ?? string.Empty;
            bookEntity.DateAdded = inputModel.DateAdded;
            bookEntity.PublisherId = inputModel.PublisherId;
            // Update BookAuthor relationships
            var existingAuthorIds = bookEntity.BooksAuthors
                .Select(ba => ba.AuthorId)
                .ToList();

            // Validate publisher exists
            if (!await DbContext.Publishers.AnyAsync(p => p.Id == inputModel.PublisherId))
            {
                throw new PublisherDoesntExistException("Selected publisher does not exist.");
            }

            // Validate provided author ids (if any) before creating the book to avoid FK errors
            if (inputModel.AuthorIds != null && inputModel.AuthorIds.Any())
            {
                var validAuthorIds = await DbContext.Authors
                    .Where(a => inputModel.AuthorIds.Contains(a.Id))
                    .Select(a => a.Id)
                    .ToListAsync();

                var invalidIds = inputModel.AuthorIds.Except(validAuthorIds).ToList();
                if (invalidIds.Any())
                {
                    throw new AuthorDoesntExistException("One or more selected authors are invalid.");
                }
            }

            try
            {
                var newAuthorIds = inputModel.AuthorIds ?? new List<Guid>();

                // Remove unselected authors
                var toRemove = bookEntity.BooksAuthors
                    .Where(ba => !newAuthorIds.Contains(ba.AuthorId))
                    .ToList();

                DbContext.BooksAuthors.RemoveRange(toRemove);

                // Add newly selected authors
                var toAdd = newAuthorIds
                    .Except(existingAuthorIds)
                    .Select(authorId => new BookAuthor
                    {
                        BookId = bookEntity.Id,
                        AuthorId = authorId
                    });

                await DbContext.BooksAuthors.AddRangeAsync(toAdd);
                await DbContext.SaveChangesAsync();

            }
            catch (Exception)
            {
                throw new InvalidOperationException("An error occurred while updating the book. Please try again.");
            }
        }

        public async Task<Book> GetBookDeleteDetailsAsync(Guid id, Guid userId)
        {
            var book = await DbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.AddedByUser)
                .Include(ba => ba.BooksAuthors)
                    .ThenInclude(ba => ba.Author)
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

            var bookToDelete = new Book
            {
                Id = book.Id,
                Title = book.Title,
                AddedByUserName = book.AddedByUser?.UserName, // null-safe access
                CoverUrl = book.CoverUrl
            };

            return bookToDelete;
        }

        public async Task DeleteBookAsync(Guid id, Guid userId)
        {
            // Load tracked entity with related collections (no AsNoTracking)
            var book = await DbContext.Books
                .Where(b => !b.IsDeleted)
                .Include(b => b.AddedByUser)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (book == null)
            {
                throw new ArgumentException("Book not found.");
            }

            if (book.AddedByUserId != userId)
            {
                throw new UnauthorizedAccessException("You are not authorized to delete this book.");
            }

            // Remove dependent BookAuthor entries
            var bookAuthorEntries = DbContext.BooksAuthors
                .Where(ba => ba.BookId == id);
            DbContext.BooksAuthors.RemoveRange(bookAuthorEntries);

            // Remove dependent UserBook entries (user collections)
            var userBookEntries = DbContext.UsersBooks
                .Where(ub => ub.BookId == id);
            DbContext.UsersBooks.RemoveRange(userBookEntries);

            // Soft-delete the book
            book.IsDeleted = true;

            await DbContext.SaveChangesAsync();
        }




    }
}
