using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Models.Publisher;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class PublisherServiceTests
    {
        private Mock<IPublisherRepository> _repoMock;
        private PublisherService _sut;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IPublisherRepository>();
            _sut = new PublisherService(_repoMock.Object);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllPublishersAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllPublishersAsync_NoFilter_ReturnsAllPublishersOrdered()
        {
            var publishers = new List<Publisher>
            {
                new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" },
                new Publisher { Id = Guid.NewGuid(), Name = "Addison Press" }
            };
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(publishers);

            var (result, totalPages) = await _sut.GetAllPublishersAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().Name, Is.EqualTo("Addison Press"));
            Assert.That(totalPages, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllPublishersAsync_WithMatchingSearchQuery_ReturnsFilteredPublishers()
        {
            var publishers = new List<Publisher>
            {
                new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" },
                new Publisher { Id = Guid.NewGuid(), Name = "Addison Press" }
            };
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(publishers);

            var (result, _) = await _sut.GetAllPublishersAsync(searchQuery: "penguin");

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().Name, Is.EqualTo("Penguin Books"));
        }

        [Test]
        public async Task GetAllPublishersAsync_WithNonMatchingSearchQuery_ReturnsEmpty()
        {
            var publishers = new List<Publisher>
            {
                new Publisher { Id = Guid.NewGuid(), Name = "Penguin Books" }
            };
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(publishers);

            var (result, totalPages) = await _sut.GetAllPublishersAsync(searchQuery: "xyz");

            Assert.That(result, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllPublishersAsync_PaginationPageOne_ReturnsCorrectSlice()
        {
            var publishers = Enumerable.Range(1, 25)
                .Select(i => new Publisher { Id = Guid.NewGuid(), Name = $"Publisher {i:D2}" })
                .ToList();
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(publishers);

            var (result, totalPages) = await _sut.GetAllPublishersAsync(pageNumber: 1, pageSize: 20);

            Assert.That(result.Count(), Is.EqualTo(20));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllPublishersAsync_PaginationPageTwo_ReturnsRemainder()
        {
            var publishers = Enumerable.Range(1, 25)
                .Select(i => new Publisher { Id = Guid.NewGuid(), Name = $"Publisher {i:D2}" })
                .ToList();
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(publishers);

            var (result, totalPages) = await _sut.GetAllPublishersAsync(pageNumber: 2, pageSize: 20);

            Assert.That(result.Count(), Is.EqualTo(5));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllPublishersAsync_EmptyRepository_ReturnsEmptyWithZeroPages()
        {
            _repoMock.Setup(r => r.GetAllPublishersAsync()).ReturnsAsync(new List<Publisher>());

            var (result, totalPages) = await _sut.GetAllPublishersAsync();

            Assert.That(result, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetPublisherDetailsByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetPublisherDetailsByIdAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetPublisherByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Publisher?)null);

            var result = await _sut.GetPublisherDetailsByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPublisherDetailsByIdAsync_ExistingId_ReturnsCorrectDto()
        {
            var publisherId = Guid.NewGuid();
            var author = new Author { Id = Guid.NewGuid(), FullName = "Test Author" };
            var bookId = Guid.NewGuid();
            var book = new Book
            {
                Id = bookId,
                Title = "Test Book",
                CoverUrl = string.Empty,
                Rating = 4,
                DateAdded = new DateTime(2024, 1, 1),
                Genre = BookGenre.Fiction,
                Description = "Desc",
                IsDeleted = false,
                AddedByUserId = Guid.NewGuid(),
                PublisherId = publisherId,
                BooksAuthors = new List<BookAuthor>
                {
                    new BookAuthor { BookId = bookId, AuthorId = author.Id, Author = author, Book = null! }
                }
            };
            var publisher = new Publisher
            {
                Id = publisherId,
                Name = "Test Publisher",
                Books = new List<Book> { book }
            };
            _repoMock.Setup(r => r.GetPublisherByIdAsync(publisherId)).ReturnsAsync(publisher);

            var result = await _sut.GetPublisherDetailsByIdAsync(publisherId);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Name, Is.EqualTo("Test Publisher"));
            Assert.That(result.BooksWithAuthorName.Count, Is.EqualTo(1));
            Assert.That(result.BooksWithAuthorName.First().Title, Is.EqualTo("Test Book"));
            Assert.That(result.BooksWithAuthorName.First().AuthorsName, Is.EqualTo("Test Author"));
        }

        [Test]
        public async Task GetPublisherDetailsByIdAsync_ExcludesDeletedBooks()
        {
            var publisherId = Guid.NewGuid();
            var publisher = new Publisher
            {
                Id = publisherId,
                Name = "Test Publisher",
                Books = new List<Book>
                {
                    new Book
                    {
                        Id = Guid.NewGuid(), Title = "Active Book", IsDeleted = false,
                        Description = "D", Genre = BookGenre.Fiction, DateAdded = DateTime.UtcNow,
                        AddedByUserId = Guid.NewGuid(), PublisherId = publisherId,
                        BooksAuthors = new List<BookAuthor>()
                    },
                    new Book
                    {
                        Id = Guid.NewGuid(), Title = "Deleted Book", IsDeleted = true,
                        Description = "D", Genre = BookGenre.Fiction, DateAdded = DateTime.UtcNow,
                        AddedByUserId = Guid.NewGuid(), PublisherId = publisherId,
                        BooksAuthors = new List<BookAuthor>()
                    }
                }
            };
            _repoMock.Setup(r => r.GetPublisherByIdAsync(publisherId)).ReturnsAsync(publisher);

            var result = await _sut.GetPublisherDetailsByIdAsync(publisherId);

            Assert.That(result!.BooksWithAuthorName.Count, Is.EqualTo(1));
            Assert.That(result.BooksWithAuthorName.First().Title, Is.EqualTo("Active Book"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetEmptyPublisherViewModel
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GetEmptyPublisherViewModel_ReturnsNewEmptyDto()
        {
            var result = _sut.GetEmptyPublisherViewModel();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<PublisherAddDto>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // AddNewPublisherAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AddNewPublisherAsync_ValidModel_CallsRepositoryOnce()
        {
            var dto = new PublisherAddDto { Name = "  New Publisher  " };
            _repoMock.Setup(r => r.AddPublisherAsync(It.IsAny<Publisher>())).Returns(Task.CompletedTask);

            await _sut.AddNewPublisherAsync(dto);

            _repoMock.Verify(r => r.AddPublisherAsync(It.Is<Publisher>(p => p.Name == "New Publisher")), Times.Once);
        }

        [Test]
        public void AddNewPublisherAsync_RepositoryThrowsAlreadyExists_PropagatesException()
        {
            var dto = new PublisherAddDto { Name = "Existing Publisher" };
            _repoMock.Setup(r => r.AddPublisherAsync(It.IsAny<Publisher>()))
                     .ThrowsAsync(new PublisherAlreadyExistsException("Existing Publisher"));

            Assert.ThrowsAsync<PublisherAlreadyExistsException>(() => _sut.AddNewPublisherAsync(dto));
        }

        [Test]
        public void AddNewPublisherAsync_RepositoryThrowsDbUpdateException_ThrowsPublisherCreateException()
        {
            var dto = new PublisherAddDto { Name = "New Publisher" };
            _repoMock.Setup(r => r.AddPublisherAsync(It.IsAny<Publisher>()))
                     .ThrowsAsync(new DbUpdateException());

            Assert.ThrowsAsync<PublisherCreateException>(() => _sut.AddNewPublisherAsync(dto));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetNewPublisherForEditByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetNewPublisherForEditByIdAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetPublisherForEditByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Publisher?)null);

            var result = await _sut.GetNewPublisherForEditByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetNewPublisherForEditByIdAsync_ExistingId_ReturnsDto()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetPublisherForEditByIdAsync(id))
                     .ReturnsAsync(new Publisher { Id = id, Name = "Edit Me" });

            var result = await _sut.GetNewPublisherForEditByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("Edit Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // UpdateNewPublisherAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task UpdateNewPublisherAsync_NonExistingId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetPublisherForEditByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Publisher?)null);

            var result = await _sut.UpdateNewPublisherAsync(Guid.NewGuid(), new PublisherAllDto { Name = "X" });

            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.UpdatePublisherAsync(It.IsAny<Guid>(), It.IsAny<Publisher>()), Times.Never);
        }

        [Test]
        public async Task UpdateNewPublisherAsync_ExistingId_CallsRepositoryAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetPublisherForEditByIdAsync(id))
                     .ReturnsAsync(new Publisher { Id = id, Name = "Old Name" });
            _repoMock.Setup(r => r.UpdatePublisherAsync(id, It.IsAny<Publisher>())).ReturnsAsync(true);

            var result = await _sut.UpdateNewPublisherAsync(id, new PublisherAllDto { Id = id, Name = "New Name" });

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.UpdatePublisherAsync(id, It.Is<Publisher>(p => p.Name == "New Name")), Times.Once);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetPublisherNewDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetPublisherNewDeleteDetailsAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetPublisherDeleteDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Publisher?)null);

            var result = await _sut.GetPublisherNewDeleteDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        [Test]
        public async Task GetPublisherNewDeleteDetailsAsync_ExistingId_ReturnsDeleteDto()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetPublisherDeleteDetailsAsync(id))
                     .ReturnsAsync(new Publisher { Id = id, Name = "Delete Me" });

            var result = await _sut.GetPublisherNewDeleteDetailsAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.Name, Is.EqualTo("Delete Me"));
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeletePublisherByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeletePublisherByIdAsync_NonExistingId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetPublisherDeleteDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Publisher?)null);

            var result = await _sut.DeletePublisherByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.DeletePublisherAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task DeletePublisherByIdAsync_PublisherWithBooks_ThrowsPublisherDeleteException()
        {
            var id = Guid.NewGuid();
            var publisher = new Publisher
            {
                Id = id,
                Name = "Has Books",
                Books = new List<Book>
                {
                    new Book
                    {
                        Id = Guid.NewGuid(), Title = "T", Description = "D",
                        Genre = BookGenre.Fiction, DateAdded = DateTime.UtcNow,
                        AddedByUserId = Guid.NewGuid(), PublisherId = id
                    }
                }
            };
            _repoMock.Setup(r => r.GetPublisherDeleteDetailsAsync(id)).ReturnsAsync(publisher);

            Assert.ThrowsAsync<PublisherDeleteException>(() => _sut.DeletePublisherByIdAsync(id));
            _repoMock.Verify(r => r.DeletePublisherAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task DeletePublisherByIdAsync_PublisherWithNoBooks_CallsRepositoryAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            _repoMock.Setup(r => r.GetPublisherDeleteDetailsAsync(id))
                     .ReturnsAsync(new Publisher { Id = id, Name = "No Books", Books = new List<Book>() });
            _repoMock.Setup(r => r.DeletePublisherAsync(id)).ReturnsAsync(true);

            var result = await _sut.DeletePublisherByIdAsync(id);

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.DeletePublisherAsync(id), Times.Once);
        }
    }
}
