using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class PublisherRepositoryTests
    {
        private OnlineLibraryDbContext CreateContext(string dbName)
        {
            var options = new DbContextOptionsBuilder<OnlineLibraryDbContext>()
                .UseInMemoryDatabase(databaseName: dbName)
                .Options;
            return new OnlineLibraryDbContext(options);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllPublishersAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllPublishersAsync_EmptyDatabase_ReturnsEmptyList()
        {
            await using var ctx = CreateContext(nameof(GetAllPublishersAsync_EmptyDatabase_ReturnsEmptyList));
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetAllPublishersAsync();

            Assert.That(result, Is.Empty);
        }

        [Test]
        public async Task GetAllPublishersAsync_MultiplePublishers_ReturnsOrderedByName()
        {
            await using var ctx = CreateContext(nameof(GetAllPublishersAsync_MultiplePublishers_ReturnsOrderedByName));
            ctx.Publishers.AddRange(
                new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" },
                new Publisher { Id = Guid.NewGuid(), Name = "Addison Press" }
            );
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = (await repo.GetAllPublishersAsync()).ToList();

            Assert.That(result.Count, Is.EqualTo(2));
            Assert.That(result[0].Name, Is.EqualTo("Addison Press"));
            Assert.That(result[1].Name, Is.EqualTo("Penguin Books"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetPublisherByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetPublisherByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetPublisherByIdAsync_NonExistingId_ReturnsNull));
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPublisherByIdAsync_ExistingId_ReturnsPublisher()
        {
            await using var ctx = CreateContext(nameof(GetPublisherByIdAsync_ExistingId_ReturnsPublisher));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "Test Publisher" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Test Publisher"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetEmptyPublisherFormModelAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetEmptyPublisherFormModelAsync_ReturnsNewPublisher()
        {
            await using var ctx = CreateContext(nameof(GetEmptyPublisherFormModelAsync_ReturnsNewPublisher));
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetEmptyPublisherFormModelAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<Publisher>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // ExistsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task ExistsAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(ExistsAsync_NonExistingId_ReturnsFalse));
            var repo = new PublisherRepository(ctx);

            var result = await repo.ExistsAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task ExistsAsync_ExistingId_ReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(ExistsAsync_ExistingId_ReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "Existing Publisher" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.ExistsAsync(id);

            Assert.That(result, Is.True);
        }

        // ──────────────────────────────────────────────────────────────────────
        // AddPublisherAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AddPublisherAsync_ValidInput_PersistsPublisherWithTrimmedName()
        {
            await using var ctx = CreateContext(nameof(AddPublisherAsync_ValidInput_PersistsPublisherWithTrimmedName));
            var repo = new PublisherRepository(ctx);

            await repo.AddPublisherAsync(new Publisher { Name = "  New Publisher  " });

            Assert.That(await ctx.Publishers.AnyAsync(p => p.Name == "New Publisher"), Is.True);
        }

        [Test]
        public async Task AddPublisherAsync_DuplicateName_ThrowsPublisherAlreadyExistsException()
        {
            await using var ctx = CreateContext(nameof(AddPublisherAsync_DuplicateName_ThrowsPublisherAlreadyExistsException));
            ctx.Publishers.Add(new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            Assert.ThrowsAsync<PublisherAlreadyExistsException>(
                () => repo.AddPublisherAsync(new Publisher { Name = "Penguin Books" }));
        }

        [Test]
        public async Task AddPublisherAsync_DuplicateNameDifferentCase_ThrowsPublisherAlreadyExistsException()
        {
            await using var ctx = CreateContext(nameof(AddPublisherAsync_DuplicateNameDifferentCase_ThrowsPublisherAlreadyExistsException));
            ctx.Publishers.Add(new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            Assert.ThrowsAsync<PublisherAlreadyExistsException>(
                () => repo.AddPublisherAsync(new Publisher { Name = "penguin books" }));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetPublisherForEditByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetPublisherForEditByIdAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetPublisherForEditByIdAsync_NonExistingId_ReturnsNull));
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherForEditByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPublisherForEditByIdAsync_ExistingId_ReturnsPublisher()
        {
            await using var ctx = CreateContext(nameof(GetPublisherForEditByIdAsync_ExistingId_ReturnsPublisher));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "Edit Me" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherForEditByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("Edit Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // UpdatePublisherAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task UpdatePublisherAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(UpdatePublisherAsync_NonExistingId_ReturnsFalse));
            var repo = new PublisherRepository(ctx);

            var result = await repo.UpdatePublisherAsync(Guid.NewGuid(), new Publisher { Name = "X" });

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task UpdatePublisherAsync_ExistingId_UpdatesNameAndReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(UpdatePublisherAsync_ExistingId_UpdatesNameAndReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "Old Name" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.UpdatePublisherAsync(id, new Publisher { Id = id, Name = "New Name" });

            Assert.That(result, Is.True);
            var updated = await ctx.Publishers.FirstOrDefaultAsync(p => p.Id == id);
            Assert.That(updated!.Name, Is.EqualTo("New Name"));
        }

        [Test]
        public async Task UpdatePublisherAsync_IdMismatch_ThrowsPublisherUpdateExeption()
        {
            await using var ctx = CreateContext(nameof(UpdatePublisherAsync_IdMismatch_ThrowsPublisherUpdateExeption));
            var repo = new PublisherRepository(ctx);

            Assert.ThrowsAsync<PublisherUpdateExeption>(
                () => repo.UpdatePublisherAsync(Guid.NewGuid(), new Publisher { Id = Guid.NewGuid(), Name = "X" }));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetPublisherDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetPublisherDeleteDetailsAsync_NonExistingId_ReturnsNull()
        {
            await using var ctx = CreateContext(nameof(GetPublisherDeleteDetailsAsync_NonExistingId_ReturnsNull));
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherDeleteDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPublisherDeleteDetailsAsync_ExistingId_ReturnsPublisher()
        {
            await using var ctx = CreateContext(nameof(GetPublisherDeleteDetailsAsync_ExistingId_ReturnsPublisher));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "Delete Me" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.GetPublisherDeleteDetailsAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Delete Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeletePublisherAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeletePublisherAsync_NonExistingId_ReturnsFalse()
        {
            await using var ctx = CreateContext(nameof(DeletePublisherAsync_NonExistingId_ReturnsFalse));
            var repo = new PublisherRepository(ctx);

            var result = await repo.DeletePublisherAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
        }

        [Test]
        public async Task DeletePublisherAsync_PublisherWithBooks_ThrowsPublisherDeleteException()
        {
            await using var ctx = CreateContext(nameof(DeletePublisherAsync_PublisherWithBooks_ThrowsPublisherDeleteException));
            var publisherId = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = publisherId, Name = "Has Books" });
            ctx.Books.Add(new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                Description = "Desc",
                Genre = BookGenre.Fiction,
                CoverUrl = string.Empty,
                Rating = 4,
                DateAdded = DateTime.UtcNow,
                PublisherId = publisherId,
                AddedByUserId = Guid.NewGuid()
            });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            Assert.ThrowsAsync<PublisherDeleteException>(() => repo.DeletePublisherAsync(publisherId));
        }

        [Test]
        public async Task DeletePublisherAsync_PublisherWithNoBooks_RemovesPublisherAndReturnsTrue()
        {
            await using var ctx = CreateContext(nameof(DeletePublisherAsync_PublisherWithNoBooks_RemovesPublisherAndReturnsTrue));
            var id = Guid.NewGuid();
            ctx.Publishers.Add(new Publisher { Id = id, Name = "No Books" });
            await ctx.SaveChangesAsync();
            var repo = new PublisherRepository(ctx);

            var result = await repo.DeletePublisherAsync(id);

            Assert.That(result, Is.True);
            Assert.That(await ctx.Publishers.AnyAsync(p => p.Id == id), Is.False);
        }
    }
}
