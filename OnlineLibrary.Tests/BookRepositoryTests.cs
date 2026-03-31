using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class BookRepositoryTests
    {
        private OnlineLibraryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineLibraryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineLibraryDbContext(options);
        }

        private static ApplicationUser MakeUser(Guid id, string userName = "testuser") =>
            new ApplicationUser { Id = id, UserName = userName };

        private static Publisher MakePublisher(Guid id, string name = "Test Publisher") =>
            new Publisher { Id = id, Name = name };

        private static Author MakeAuthor(Guid id, string fullName = "Test Author") =>
            new Author { Id = id, FullName = fullName };

        private static Book MakeBook(Guid id, string title, Guid publisherId, Guid addedByUserId, bool isDeleted = false) =>
            new Book
            {
                Id = id,
                Title = title,
                Description = "A description",
                Genre = BookGenre.Fiction,
                IsRead = false,
                Rating = 5,
                CoverUrl = "http://example.com/cover.jpg",
                DateAdded = DateTime.UtcNow,
                PublisherId = publisherId,
                AddedByUserId = addedByUserId,
                IsDeleted = isDeleted
            };

        // ──────────────────────────────────────────────────────────────────────
        // GetAllBooksOrderedByTitleThenByGenreAscAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllBooksOrderedByTitleThenByGenreAscAsync_EmptyDatabase_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksOrderedByTitleThenByGenreAscAsync_EmptyDatabase_ReturnsEmptyList));
            var repo = new BookRepository(ctx);

            var result = await repo.GetAllBooksOrderedByTitleThenByGenreAscAsync(null);

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllBooksOrderedByTitleThenByGenreAscAsync_DeletedBooksExist_ReturnsOnlyNonDeleted()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksOrderedByTitleThenByGenreAscAsync_DeletedBooksExist_ReturnsOnlyNonDeleted));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.AddRange(
                MakeBook(Guid.NewGuid(), "Active Book", pubId, userId, isDeleted: false),
                MakeBook(Guid.NewGuid(), "Deleted Book", pubId, userId, isDeleted: true)
            );
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetAllBooksOrderedByTitleThenByGenreAscAsync(null)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Active Book"));
        }

        [Test]
        public async Task GetAllBooksOrderedByTitleThenByGenreAscAsync_MultipleNonDeletedBooks_ReturnsAll()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksOrderedByTitleThenByGenreAscAsync_MultipleNonDeletedBooks_ReturnsAll));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.AddRange(
                MakeBook(Guid.NewGuid(), "Zulu", pubId, userId),
                MakeBook(Guid.NewGuid(), "Alpha", pubId, userId)
            );
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetAllBooksOrderedByTitleThenByGenreAscAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBooksByUserOrderedByTitleThenByGenreAscAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBooksByUserOrderedByTitleThenByGenreAscAsync_EmptyDatabase_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetBooksByUserOrderedByTitleThenByGenreAscAsync_EmptyDatabase_ReturnsEmptyList));
            var repo = new BookRepository(ctx);

            var result = await repo.GetBooksByUserOrderedByTitleThenByGenreAscAsync(Guid.NewGuid());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetBooksByUserOrderedByTitleThenByGenreAscAsync_DeletedBooksExist_ReturnsOnlyNonDeleted()
        {
            await using var ctx = CreateContext(nameof(GetBooksByUserOrderedByTitleThenByGenreAscAsync_DeletedBooksExist_ReturnsOnlyNonDeleted));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.AddRange(
                MakeBook(Guid.NewGuid(), "My Active Book", pubId, userId, isDeleted: false),
                MakeBook(Guid.NewGuid(), "My Deleted Book", pubId, userId, isDeleted: true)
            );
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetBooksByUserOrderedByTitleThenByGenreAscAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("My Active Book"));
        }

        [Test]
        public async Task GetBooksByUserOrderedByTitleThenByGenreAscAsync_ReturnsOnlyCurrentUserNonDeletedBooks()
        {
            await using var ctx = CreateContext(nameof(GetBooksByUserOrderedByTitleThenByGenreAscAsync_ReturnsOnlyCurrentUserNonDeletedBooks));
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var pubId = Guid.NewGuid();

            ctx.Users.AddRange(MakeUser(userId, "owner"), MakeUser(otherUserId, "other"));
            ctx.Publishers.Add(MakePublisher(pubId));

            ctx.Books.AddRange(
                MakeBook(Guid.NewGuid(), "My Active Book", pubId, userId, isDeleted: false),
                MakeBook(Guid.NewGuid(), "My Deleted Book", pubId, userId, isDeleted: true),
                MakeBook(Guid.NewGuid(), "Other User Book", pubId, otherUserId, isDeleted: false)
            );

            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetBooksByUserOrderedByTitleThenByGenreAscAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("My Active Book"));
            Assert.That(result[0].AddedByUserId, Is.EqualTo(userId));
        }

        [Test]
        public async Task GetBooksByUserOrderedByTitleThenByGenreAscAsync_PopulatesUsersBooksOnlyForCurrentUser()
        {
            await using var ctx = CreateContext(nameof(GetBooksByUserOrderedByTitleThenByGenreAscAsync_PopulatesUsersBooksOnlyForCurrentUser));
            var userId = Guid.NewGuid();
            var otherUserId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();

            ctx.Users.AddRange(MakeUser(userId, "owner"), MakeUser(otherUserId, "other"));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));

            ctx.UsersBooks.AddRange(
                new UserBook { UserId = userId, BookId = bookId },
                new UserBook { UserId = otherUserId, BookId = bookId }
            );

            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetBooksByUserOrderedByTitleThenByGenreAscAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].UsersBooks.Count, Is.EqualTo(1));
            Assert.That(result[0].UsersBooks.First().UserId, Is.EqualTo(userId));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorsAndPublishersAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorsAndPublishersAsync_ReturnsPublishersAndAuthorsOrderedByName()
        {
            await using var ctx = CreateContext(nameof(GetAuthorsAndPublishersAsync_ReturnsPublishersAndAuthorsOrderedByName));
            ctx.Publishers.AddRange(
                new Publisher { Id = Guid.NewGuid(), Name = "Zebra Press" },
                new Publisher { Id = Guid.NewGuid(), Name = "Alpha Books" }
            );
            ctx.Authors.AddRange(
                new Author { Id = Guid.NewGuid(), FullName = "Zara Smith" },
                new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" }
            );
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var (publishers, authors) = await repo.GetAuthorsAndPublishersAsync();

            var publisherList = publishers.ToList();
            var authorList = authors.ToList();
            Assert.That(publisherList[0].Name, Is.EqualTo("Alpha Books"));
            Assert.That(publisherList[1].Name, Is.EqualTo("Zebra Press"));
            Assert.That(authorList[0].FullName, Is.EqualTo("Alan Poe"));
            Assert.That(authorList[1].FullName, Is.EqualTo("Zara Smith"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookDetailsByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookDetailsByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookDetailsByIdAsync_NonExistingId_ReturnsNull));
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookDetailsByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookDetailsByIdAsync_DeletedBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookDetailsByIdAsync_DeletedBook_ReturnsNull));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Deleted Book", pubId, userId, isDeleted: true));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookDetailsByIdAsync(bookId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookDetailsByIdAsync_ExistingNonDeletedBook_ReturnsCorrectBook()
        {
            await using var ctx = CreateContext(nameof(GetBookDetailsByIdAsync_ExistingNonDeletedBook_ReturnsCorrectBook));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Test Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookDetailsByIdAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Test Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // IsBookAddedByUserAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task IsBookAddedByUserAsync_CorrectUser_ReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(IsBookAddedByUserAsync_CorrectUser_ReturnsTrue));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.IsBookAddedByUserAsync(userId, bookId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsBookAddedByUserAsync_WrongUser_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(IsBookAddedByUserAsync_WrongUser_ReturnsFalse));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.IsBookAddedByUserAsync(Guid.NewGuid(), bookId);

            Assert.That(result, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // IsBookAddedToUserCollectionAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task IsBookAddedToUserCollectionAsync_BookInCollection_ReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(IsBookAddedToUserCollectionAsync_BookInCollection_ReturnsTrue));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "A Book", pubId, userId));
            ctx.UsersBooks.Add(new UserBook { UserId = userId, BookId = bookId });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.IsBookAddedToUserCollectionAsync(userId, bookId);

            Assert.That(result, Is.True);
        }

        [Test]
        public async Task IsBookAddedToUserCollectionAsync_NullUserId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(IsBookAddedToUserCollectionAsync_NullUserId_ReturnsFalse));
            var repo = new BookRepository(ctx);

            var result = await repo.IsBookAddedToUserCollectionAsync(null, Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task IsBookAddedToUserCollectionAsync_BookNotInCollection_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(IsBookAddedToUserCollectionAsync_BookNotInCollection_ReturnsFalse));
            var userId = Guid.NewGuid();
            var repo = new BookRepository(ctx);

            var result = await repo.IsBookAddedToUserCollectionAsync(userId, Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        // ──────────────────────────────────────────────────────────────────────
        // CreateBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task CreateBookAsync_InvalidPublisher_ThrowsPublisherDoesntExistException()
        {
            await using var ctx = CreateContext(nameof(CreateBookAsync_InvalidPublisher_ThrowsPublisherDoesntExistException));
            var repo = new BookRepository(ctx);
            var book = MakeBook(Guid.NewGuid(), "New Book", Guid.NewGuid(), Guid.NewGuid());

            Assert.ThrowsAsync<PublisherDoesntExistException>(
                async () => await repo.CreateBookAsync(book, Guid.NewGuid()));
        }

        [Test]
        public async Task CreateBookAsync_InvalidAuthorId_ThrowsAuthorDoesntExistException()
        {
            await using var ctx = CreateContext(nameof(CreateBookAsync_InvalidAuthorId_ThrowsAuthorDoesntExistException));
            var pubId = Guid.NewGuid();
            ctx.Publishers.Add(MakePublisher(pubId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);
            var book = MakeBook(Guid.NewGuid(), "New Book", pubId, Guid.NewGuid());
            book.BooksAuthors = new HashSet<BookAuthor>
            {
                new BookAuthor { AuthorId = Guid.NewGuid() }
            };

            Assert.ThrowsAsync<AuthorDoesntExistException>(
                async () => await repo.CreateBookAsync(book, Guid.NewGuid()));
        }

        [Test]
        public async Task CreateBookAsync_ValidData_PersistsBookInDatabase()
        {
            await using var ctx = CreateContext(nameof(CreateBookAsync_ValidData_PersistsBookInDatabase));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);
            var book = MakeBook(Guid.NewGuid(), "New Book", pubId, userId);

            await repo.CreateBookAsync(book, userId);

            Assert.That(ctx.Books.Count(), Is.EqualTo(1));
            Assert.That(ctx.Books.First().Title, Is.EqualTo("New Book"));
        }

        [Test]
        public async Task CreateBookAsync_WithNoAuthors_PersistsBookWithEmptyAuthorLinks()
        {
            await using var ctx = CreateContext(nameof(CreateBookAsync_WithNoAuthors_PersistsBookWithEmptyAuthorLinks));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            // Mirrors how BooksService calls CreateBookAsync: no BooksAuthors set
            var book = MakeBook(Guid.NewGuid(), "No-Author Book", pubId, userId);

            await repo.CreateBookAsync(book, userId);

            Assert.That(ctx.Books.Count(), Is.EqualTo(1));
            Assert.That(ctx.BooksAuthors.Count(), Is.EqualTo(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetFavoriteBooksAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetFavoriteBooksAsync_NoFavorites_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetFavoriteBooksAsync_NoFavorites_ReturnsEmptyList));
            var repo = new BookRepository(ctx);

            var result = await repo.GetFavoriteBooksAsync(Guid.NewGuid());

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetFavoriteBooksAsync_WithFavorites_ReturnsUserFavorites()
        {
            await using var ctx = CreateContext(nameof(GetFavoriteBooksAsync_WithFavorites_ReturnsUserFavorites));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Fav Book", pubId, userId));
            ctx.UsersBooks.Add(new UserBook { UserId = userId, BookId = bookId });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetFavoriteBooksAsync(userId)).ToList();

            Assert.That(result.Count, Is.EqualTo(1));
            Assert.That(result[0].Title, Is.EqualTo("Fav Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // SaveFevBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task SaveFevBookAsync_NotInCollection_AddsEntry()
        {
            await using var ctx = CreateContext(nameof(SaveFevBookAsync_NotInCollection_AddsEntry));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "A Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.SaveFevBookAsync(bookId, userId);

            Assert.That(ctx.UsersBooks.Count(), Is.EqualTo(1));
        }

        [Test]
        public async Task SaveFevBookAsync_AlreadyInCollection_DoesNotAddDuplicate()
        {
            await using var ctx = CreateContext(nameof(SaveFevBookAsync_AlreadyInCollection_DoesNotAddDuplicate));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "A Book", pubId, userId));
            ctx.UsersBooks.Add(new UserBook { UserId = userId, BookId = bookId });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.SaveFevBookAsync(bookId, userId);

            Assert.That(ctx.UsersBooks.Count(), Is.EqualTo(1));
        }

        // ──────────────────────────────────────────────────────────────────────
        // RemoveFevBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task RemoveFevBookAsync_ExistingEntry_RemovesFromCollection()
        {
            await using var ctx = CreateContext(nameof(RemoveFevBookAsync_ExistingEntry_RemovesFromCollection));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Fav Book", pubId, userId));
            ctx.UsersBooks.Add(new UserBook { UserId = userId, BookId = bookId });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.RemoveFevBookAsync(bookId, userId);

            Assert.That(ctx.UsersBooks.Count(), Is.EqualTo(0));
        }

        [Test]
        public async Task RemoveFevBookAsync_NonExistingEntry_DoesNotThrow()
        {
            await using var ctx = CreateContext(nameof(RemoveFevBookAsync_NonExistingEntry_DoesNotThrow));
            var repo = new BookRepository(ctx);

            Assert.DoesNotThrowAsync(async () =>
                await repo.RemoveFevBookAsync(Guid.NewGuid(), Guid.NewGuid()));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookForEditAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookForEditAsync_CorrectOwner_ReturnsBook()
        {
            await using var ctx = CreateContext(nameof(GetBookForEditAsync_CorrectOwner_ReturnsBook));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForEditAsync(bookId, userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
        }

        [Test]
        public async Task GetBookForEditAsync_WrongOwner_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookForEditAsync_WrongOwner_ReturnsNull));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForEditAsync(bookId, Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookForEditAsync_DeletedBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookForEditAsync_DeletedBook_ReturnsNull));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Deleted Book", pubId, userId, isDeleted: true));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForEditAsync(bookId, userId);

            Assert.That(result, Is.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // EditBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task EditBookAsync_NonExistingBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(EditBookAsync_NonExistingBook_ReturnsFalse));
            var repo = new BookRepository(ctx);
            var book = MakeBook(Guid.NewGuid(), "Updated", Guid.NewGuid(), Guid.NewGuid());

            var result = await repo.EditBookAsync(book, Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EditBookAsync_WrongOwner_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(EditBookAsync_WrongOwner_ReturnsFalse));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);
            var inputModel = MakeBook(bookId, "Updated", pubId, userId);

            var result = await repo.EditBookAsync(inputModel, Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EditBookAsync_InvalidPublisher_ThrowsPublisherDoesntExistException()
        {
            await using var ctx = CreateContext(nameof(EditBookAsync_InvalidPublisher_ThrowsPublisherDoesntExistException));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);
            var inputModel = MakeBook(bookId, "Updated", Guid.NewGuid(), userId); // invalid publisher

            Assert.ThrowsAsync<PublisherDoesntExistException>(
                async () => await repo.EditBookAsync(inputModel, userId));
        }

        [Test]
        public async Task EditBookAsync_ValidData_ReturnsTrueAndUpdatesTitle()
        {
            await using var ctx = CreateContext(nameof(EditBookAsync_ValidData_ReturnsTrueAndUpdatesTitle));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Original Title", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);
            var inputModel = MakeBook(bookId, "Updated Title", pubId, userId);

            var result = await repo.EditBookAsync(inputModel, userId);

            Assert.That(result, Is.True);
            var updated = await ctx.Books.FindAsync(bookId);
            Assert.That(updated!.Title, Is.EqualTo("Updated Title"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookDeleteDetailsAsync_NonExistingBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookDeleteDetailsAsync_NonExistingBook_ReturnsNull));
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookDeleteDetailsAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookDeleteDetailsAsync_WrongOwner_ThrowsUnauthorizedAccessException()
        {
            await using var ctx = CreateContext(nameof(GetBookDeleteDetailsAsync_WrongOwner_ThrowsUnauthorizedAccessException));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await repo.GetBookDeleteDetailsAsync(bookId, Guid.NewGuid()));
        }

        [Test]
        public async Task GetBookDeleteDetailsAsync_CorrectOwner_ReturnsBookDetails()
        {
            await using var ctx = CreateContext(nameof(GetBookDeleteDetailsAsync_CorrectOwner_ReturnsBookDetails));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookDeleteDetailsAsync(bookId, userId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("My Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteBookAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteBookAsync_NonExistingBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(DeleteBookAsync_NonExistingBook_ReturnsFalse));
            var repo = new BookRepository(ctx);

            var result = await repo.DeleteBookAsync(Guid.NewGuid(), Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteBookAsync_WrongOwner_ThrowsUnauthorizedAccessException()
        {
            await using var ctx = CreateContext(nameof(DeleteBookAsync_WrongOwner_ThrowsUnauthorizedAccessException));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            Assert.ThrowsAsync<UnauthorizedAccessException>(
                async () => await repo.DeleteBookAsync(bookId, Guid.NewGuid()));
        }

        [Test]
        public async Task DeleteBookAsync_CorrectOwner_ReturnsTrueAndSoftDeletesBook()
        {
            await using var ctx = CreateContext(nameof(DeleteBookAsync_CorrectOwner_ReturnsTrueAndSoftDeletesBook));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.DeleteBookAsync(bookId, userId);

            Assert.That(result, Is.True);
            var deleted = await ctx.Books.FindAsync(bookId);
            Assert.That(deleted!.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteBookAsync_WithBookAuthorEntries_SoftDeletesBookAuthors()
        {
            await using var ctx = CreateContext(nameof(DeleteBookAsync_WithBookAuthorEntries_SoftDeletesBookAuthors));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Authors.Add(MakeAuthor(authorId));
            ctx.Books.Add(MakeBook(bookId, "My Book", pubId, userId));
            ctx.BooksAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId, IsDeleted = false });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.DeleteBookAsync(bookId, userId);

            var bookAuthor = await ctx.BooksAuthors.FirstAsync(ba => ba.BookId == bookId);
            Assert.That(bookAuthor.IsDeleted, Is.True);
        }
    }
}
