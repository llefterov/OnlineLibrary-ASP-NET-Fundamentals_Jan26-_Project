using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AuthorRepositoryTests
    {
        private OnlineLibraryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineLibraryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineLibraryDbContext(options);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllAuthorsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllAuthorsAsync_EmptyDatabase_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetAllAuthorsAsync_EmptyDatabase_ReturnsEmptyList));
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAllAuthorsAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllAuthorsAsync_MultipleAuthors_ReturnsOrderedByFullName()
        {
            await using var ctx = CreateContext(nameof(GetAllAuthorsAsync_MultipleAuthors_ReturnsOrderedByFullName));
            ctx.Authors.AddRange(
                new Author { Id = Guid.NewGuid(), FullName = "Zara Smith" },
                new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" }
            );
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = (await repo.GetAllAuthorsAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].FullName, Is.EqualTo("Alan Poe"));
            Assert.That(result[1].FullName, Is.EqualTo("Zara Smith"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetAuthorByIdAsync_NonExistingId_ReturnsNull));
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAuthorByIdAsync_ExistingId_ReturnsAuthor()
        {
            await using var ctx = CreateContext(nameof(GetAuthorByIdAsync_ExistingId_ReturnsAuthor));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "Test Author" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("Test Author"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // ExistsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(ExistsAsync_NonExistingId_ReturnsFalse));
            var repo = new AuthorRepository(ctx);

            var result = await repo.ExistsAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(ExistsAsync_ExistingId_ReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "Existing Author" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.ExistsAsync(id);

            Assert.That(result, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // AddAuthorAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AddAuthorAsync_ValidInput_PersistsAuthorWithTrimmedName()
        {
            await using var ctx = CreateContext(nameof(AddAuthorAsync_ValidInput_PersistsAuthorWithTrimmedName));
            var repo = new AuthorRepository(ctx);

            await repo.AddAuthorAsync(new Author { FullName = "  New Author  " });

            Assert.That(await ctx.Authors.AnyAsync(a => a.FullName == "New Author"), Is.True);
        }

        [Test]
        public async Task AddAuthorAsync_DuplicateName_ThrowsAuthorAlreadyExistsException()
        {
            await using var ctx = CreateContext(nameof(AddAuthorAsync_DuplicateName_ThrowsAuthorAlreadyExistsException));
            ctx.Authors.Add(new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            Assert.ThrowsAsync<AuthorAlreadyExistsException>(
                () => repo.AddAuthorAsync(new Author { FullName = "Alan Poe" }));
        }

        [Test]
        public async Task AddAuthorAsync_DuplicateNameDifferentCase_ThrowsAuthorAlreadyExistsException()
        {
            await using var ctx = CreateContext(nameof(AddAuthorAsync_DuplicateNameDifferentCase_ThrowsAuthorAlreadyExistsException));
            ctx.Authors.Add(new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            Assert.ThrowsAsync<AuthorAlreadyExistsException>(
                () => repo.AddAuthorAsync(new Author { FullName = "alan poe" }));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorForEditByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorForEditByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetAuthorForEditByIdAsync_NonExistingId_ReturnsNull));
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorForEditByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAuthorForEditByIdAsync_ExistingId_ReturnsAuthor()
        {
            await using var ctx = CreateContext(nameof(GetAuthorForEditByIdAsync_ExistingId_ReturnsAuthor));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "Edit Me" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorForEditByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.FullName, Is.EqualTo("Edit Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // UpdateAuthorAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task UpdateAuthorAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(UpdateAuthorAsync_NonExistingId_ReturnsFalse));
            var repo = new AuthorRepository(ctx);

            var result = await repo.UpdateAuthorAsync(Guid.NewGuid(), new Author { Id = Guid.NewGuid(), FullName = "X" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdateAuthorAsync_ExistingId_UpdatesNameAndReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(UpdateAuthorAsync_ExistingId_UpdatesNameAndReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "Old Name" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.UpdateAuthorAsync(id, new Author { Id = id, FullName = "New Name" });

            Assert.That(result, Is.True);
            var updated = await ctx.Authors.FirstOrDefaultAsync(a => a.Id == id);
            Assert.That(updated!.FullName, Is.EqualTo("New Name"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorDeleteDetailsAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetAuthorDeleteDetailsAsync_NonExistingId_ReturnsNull));
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorDeleteDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetAuthorDeleteDetailsAsync_ExistingId_ReturnsAuthor()
        {
            await using var ctx = CreateContext(nameof(GetAuthorDeleteDetailsAsync_ExistingId_ReturnsAuthor));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "Delete Me" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.GetAuthorDeleteDetailsAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("Delete Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteAuthorAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteAuthorAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(DeleteAuthorAsync_NonExistingId_ReturnsFalse));
            var repo = new AuthorRepository(ctx);

            var result = await repo.DeleteAuthorAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeleteAuthorAsync_AuthorWithBooks_ThrowsAuthorDeleteException()
        {
            await using var ctx = CreateContext(nameof(DeleteAuthorAsync_AuthorWithBooks_ThrowsAuthorDeleteException));
            var authorId = Guid.NewGuid();
            var publisherId = Guid.NewGuid();
            var bookId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            ctx.Authors.Add(new Author { Id = authorId, FullName = "Has Books" });
            ctx.Publishers.Add(new Publisher { Id = publisherId, Name = "Test Publisher" });
            ctx.Books.Add(new Book
            {
                Id = bookId,
                Title = "Test Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                CoverUrl = string.Empty,
                Rating = 4,
                DateAdded = DateTime.UtcNow,
                PublisherId = publisherId,
                AddedByUserId = userId
            });
            ctx.BooksAuthors.Add(new BookAuthor { BookId = bookId, AuthorId = authorId });
            await ctx.SaveChangesAsync();

            var repo = new AuthorRepository(ctx);

            Assert.ThrowsAsync<AuthorDeleteException>(() => repo.DeleteAuthorAsync(authorId));
        }

        [Test]
        public async Task DeleteAuthorAsync_AuthorWithNoBooks_RemovesAuthorAndReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(DeleteAuthorAsync_AuthorWithNoBooks_RemovesAuthorAndReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Authors.Add(new Author { Id = id, FullName = "No Books" });
            await ctx.SaveChangesAsync();
            var repo = new AuthorRepository(ctx);

            var result = await repo.DeleteAuthorAsync(id);

            Assert.That(result, Is.True);
            Assert.That(await ctx.Authors.AnyAsync(a => a.Id == id), Is.False);
        }
    }
}
