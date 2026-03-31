using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AdminBookRepositoryTests
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
        // GetBookForAdminEditAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookForAdminEditAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookForAdminEditAsync_NonExistingId_ReturnsNull));
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForAdminEditAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookForAdminEditAsync_DeletedBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookForAdminEditAsync_DeletedBook_ReturnsNull));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Deleted Book", pubId, userId, isDeleted: true));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForAdminEditAsync(bookId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookForAdminEditAsync_ValidBook_ReturnsBook()
        {
            await using var ctx = CreateContext(nameof(GetBookForAdminEditAsync_ValidBook_ReturnsBook));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Active Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookForAdminEditAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
        }

        [Test]
        public async Task GetBookForAdminEditAsync_AnyOwner_ReturnsBook()
        {
            // Admin edit bypasses ownership check
            await using var ctx = CreateContext(nameof(GetBookForAdminEditAsync_AnyOwner_ReturnsBook));
            var ownerA = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(ownerA));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Book By A", pubId, ownerA));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            // Different admin user can still retrieve any book
            var result = await repo.GetBookForAdminEditAsync(bookId);

            Assert.That(result, Is.Not.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // EditBookForAdminAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task EditBookForAdminAsync_NonExistingBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(EditBookForAdminAsync_NonExistingBook_ReturnsFalse));
            var repo = new BookRepository(ctx);
            var book = MakeBook(Guid.NewGuid(), "Updated", Guid.NewGuid(), Guid.NewGuid());

            var result = await repo.EditBookForAdminAsync(book);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task EditBookForAdminAsync_InvalidPublisher_ThrowsPublisherDoesntExistException()
        {
            await using var ctx = CreateContext(nameof(EditBookForAdminAsync_InvalidPublisher_ThrowsPublisherDoesntExistException));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var updated = MakeBook(bookId, "Updated", Guid.NewGuid(), userId); // invalid publisher

            Assert.ThrowsAsync<PublisherDoesntExistException>(
                async () => await repo.EditBookForAdminAsync(updated));
        }

        [Test]
        public async Task EditBookForAdminAsync_ValidData_ReturnsTrueAndUpdatesTitle()
        {
            await using var ctx = CreateContext(nameof(EditBookForAdminAsync_ValidData_ReturnsTrueAndUpdatesTitle));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Original", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var updated = MakeBook(bookId, "Admin Updated", pubId, userId);
            var result = await repo.EditBookForAdminAsync(updated);

            Assert.That(result, Is.True);
            var persisted = await ctx.Books.FindAsync(bookId);
            Assert.That(persisted!.Title, Is.EqualTo("Admin Updated"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetBookAdminDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetBookAdminDeleteDetailsAsync_NonExistingBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookAdminDeleteDetailsAsync_NonExistingBook_ReturnsNull));
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookAdminDeleteDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookAdminDeleteDetailsAsync_DeletedBook_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetBookAdminDeleteDetailsAsync_DeletedBook_ReturnsNull));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Deleted", pubId, userId, isDeleted: true));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookAdminDeleteDetailsAsync(bookId);

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetBookAdminDeleteDetailsAsync_ValidBook_ReturnsDetails()
        {
            await using var ctx = CreateContext(nameof(GetBookAdminDeleteDetailsAsync_ValidBook_ReturnsDetails));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Active Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.GetBookAdminDeleteDetailsAsync(bookId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(bookId));
            Assert.That(result.Title, Is.EqualTo("Active Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteBookForAdminAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteBookForAdminAsync_NonExistingBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(DeleteBookForAdminAsync_NonExistingBook_ReturnsFalse));
            var repo = new BookRepository(ctx);

            var result = await repo.DeleteBookForAdminAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteBookForAdminAsync_ValidBook_ReturnsTrueAndSoftDeletes()
        {
            await using var ctx = CreateContext(nameof(DeleteBookForAdminAsync_ValidBook_ReturnsTrueAndSoftDeletes));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Book", pubId, userId));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.DeleteBookForAdminAsync(bookId);

            Assert.That(result, Is.True);
            var deleted = await ctx.Books.FindAsync(bookId);
            Assert.That(deleted!.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteBookForAdminAsync_WithBookAuthors_SoftDeletesBookAuthors()
        {
            await using var ctx = CreateContext(nameof(DeleteBookForAdminAsync_WithBookAuthors_SoftDeletesBookAuthors));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Authors.Add(MakeAuthor(authorId));
            ctx.Books.Add(MakeBook(bookId, "Book", pubId, userId));
            ctx.BooksAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId, IsDeleted = false });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.DeleteBookForAdminAsync(bookId);

            var ba = await ctx.BooksAuthors.FirstAsync(ba => ba.BookId == bookId);
            Assert.That(ba.IsDeleted, Is.True);
        }

        [Test]
        public async Task DeleteBookForAdminAsync_RemovesUserBookEntries()
        {
            await using var ctx = CreateContext(nameof(DeleteBookForAdminAsync_RemovesUserBookEntries));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Book", pubId, userId));
            ctx.UsersBooks.Add(new UserBook { UserId = userId, BookId = bookId });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.DeleteBookForAdminAsync(bookId);

            Assert.That(ctx.UsersBooks.Count(), Is.EqualTo(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllBooksForAdminAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllBooksForAdminAsync_EmptyDatabase_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksForAdminAsync_EmptyDatabase_ReturnsEmptyList));
            var repo = new BookRepository(ctx);

            var result = await repo.GetAllBooksForAdminAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllBooksForAdminAsync_IncludesDeletedBooks()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksForAdminAsync_IncludesDeletedBooks));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.AddRange(
                MakeBook(Guid.NewGuid(), "Active", pubId, userId, isDeleted: false),
                MakeBook(Guid.NewGuid(), "Deleted", pubId, userId, isDeleted: true)
            );
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = (await repo.GetAllBooksForAdminAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllBooksForAdminAsync_OrderedByTitleThenGenre()
        {
            await using var ctx = CreateContext(nameof(GetAllBooksForAdminAsync_OrderedByTitleThenGenre));
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

            var result = (await repo.GetAllBooksForAdminAsync()).ToList();

            Assert.That(result[0].Title, Is.EqualTo("Alpha"));
            Assert.That(result[1].Title, Is.EqualTo("Zulu"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // RestoreBookForAdminAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task RestoreBookForAdminAsync_NonExistingBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(RestoreBookForAdminAsync_NonExistingBook_ReturnsFalse));
            var repo = new BookRepository(ctx);

            var result = await repo.RestoreBookForAdminAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task RestoreBookForAdminAsync_NonDeletedBook_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(RestoreBookForAdminAsync_NonDeletedBook_ReturnsFalse));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Active Book", pubId, userId, isDeleted: false));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.RestoreBookForAdminAsync(bookId);

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task RestoreBookForAdminAsync_DeletedBook_ReturnsTrueAndSetsIsDeletedFalse()
        {
            await using var ctx = CreateContext(nameof(RestoreBookForAdminAsync_DeletedBook_ReturnsTrueAndSetsIsDeletedFalse));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Books.Add(MakeBook(bookId, "Deleted Book", pubId, userId, isDeleted: true));
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            var result = await repo.RestoreBookForAdminAsync(bookId);

            Assert.That(result, Is.True);
            var restored = await ctx.Books.FindAsync(bookId);
            Assert.That(restored!.IsDeleted, Is.False);
        }

        [Test]
        public async Task RestoreBookForAdminAsync_WithDeletedBookAuthors_RestoresBookAuthors()
        {
            await using var ctx = CreateContext(nameof(RestoreBookForAdminAsync_WithDeletedBookAuthors_RestoresBookAuthors));
            var userId = Guid.NewGuid();
            var pubId = Guid.NewGuid();
            var authorId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            ctx.Users.Add(MakeUser(userId));
            ctx.Publishers.Add(MakePublisher(pubId));
            ctx.Authors.Add(MakeAuthor(authorId));
            ctx.Books.Add(MakeBook(bookId, "Deleted Book", pubId, userId, isDeleted: true));
            ctx.BooksAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId, IsDeleted = true });
            await ctx.SaveChangesAsync();
            var repo = new BookRepository(ctx);

            await repo.RestoreBookForAdminAsync(bookId);

            var ba = await ctx.BooksAuthors.FirstAsync(ba => ba.BookId == bookId);
            Assert.That(ba.IsDeleted, Is.False);
        }
    }
}
