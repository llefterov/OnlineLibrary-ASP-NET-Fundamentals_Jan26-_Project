using Microsoft.EntityFrameworkCore;
using Moq;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core;
using OnlineLibrary.Services.Models.Author;
using NUnit.Framework;

namespace OnlineLibrary.Tests
{
    [TestFixture]
    public class AuthorServiceTests
    {
        private Mock<IAuthorRepository> _repoMock;
        private AuthorService _sut;

        [SetUp]
        public void Setup()
        {
            _repoMock = new Mock<IAuthorRepository>();
            _sut = new AuthorService(_repoMock.Object);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAllAuthorsForViewModelAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_NoFilter_ReturnsAllAuthorsOrdered()
        {
            var authors = new List<Author>
            {
                new Author { Id = Guid.NewGuid(), FullName = "Zara Smith" },
                new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" }
            };
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(authors);

            var (result, totalPages) = await _sut.GetAllAuthorsForViewModelAsync();

            Assert.That(result.Count(), Is.EqualTo(2));
            Assert.That(result.First().FullName, Is.EqualTo("Alan Poe"));
            Assert.That(totalPages, Is.EqualTo(1));
        }

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_WithMatchingSearchQuery_ReturnsFilteredAuthors()
        {
            var authors = new List<Author>
            {
                new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" },
                new Author { Id = Guid.NewGuid(), FullName = "John Doe" }
            };
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(authors);

            var (result, _) = await _sut.GetAllAuthorsForViewModelAsync(searchQuery: "alan");

            Assert.That(result.Count(), Is.EqualTo(1));
            Assert.That(result.First().FullName, Is.EqualTo("Alan Poe"));
        }

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_WithNonMatchingSearchQuery_ReturnsEmpty()
        {
            var authors = new List<Author>
            {
                new Author { Id = Guid.NewGuid(), FullName = "Alan Poe" }
            };
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(authors);

            var (result, totalPages) = await _sut.GetAllAuthorsForViewModelAsync(searchQuery: "xyz");

            Assert.That(result, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_PaginationPageOne_ReturnsCorrectSlice()
        {
            var authors = Enumerable.Range(1, 25)
                .Select(i => new Author { Id = Guid.NewGuid(), FullName = $"Author {i:D2}" })
                .ToList();
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(authors);

            var (result, totalPages) = await _sut.GetAllAuthorsForViewModelAsync(pageNumber: 1, pageSize: 20);

            Assert.That(result.Count(), Is.EqualTo(20));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_PaginationPageTwo_ReturnsRemainder()
        {
            var authors = Enumerable.Range(1, 25)
                .Select(i => new Author { Id = Guid.NewGuid(), FullName = $"Author {i:D2}" })
                .ToList();
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(authors);

            var (result, totalPages) = await _sut.GetAllAuthorsForViewModelAsync(pageNumber: 2, pageSize: 20);

            Assert.That(result.Count(), Is.EqualTo(5));
            Assert.That(totalPages, Is.EqualTo(2));
        }

        [Test]
        public async Task GetAllAuthorsForViewModelAsync_EmptyRepository_ReturnsEmptyWithZeroPages()
        {
            _repoMock.Setup(r => r.GetAllAuthorsAsync()).ReturnsAsync(new List<Author>());

            var (result, totalPages) = await _sut.GetAllAuthorsForViewModelAsync();

            Assert.That(result, Is.Empty);
            Assert.That(totalPages, Is.EqualTo(0));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorDetailsByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorDetailsByIdAsync_ExistingId_ReturnsCorrectDto()
        {
            var id = Guid.NewGuid();
            var book = new Book
            {
                Id = Guid.NewGuid(),
                Title = "Test Book",
                CoverUrl = string.Empty,
                Rating = 4,
                DateAdded = new DateTime(2024, 1, 1),
                Genre = BookGenre.Fiction,
                Publisher = new Publisher { Name = "Test Publisher" },
                Description = "Desc"
            };
            var author = new Author
            {
                Id = id,
                FullName = "Alan Poe",
                BooksAuthors = new List<BookAuthor>
                {
                    new BookAuthor { BookId = book.Id, Book = book, AuthorId = id, Author = null! }
                }
            };
            _repoMock.Setup(r => r.GetAuthorByIdAsync(id)).ReturnsAsync(author);

            var result = await _sut.GetAuthorDetailsByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.FullName, Is.EqualTo("Alan Poe"));
            Assert.That(result.BooksWithPublisherName.Count, Is.EqualTo(1));
            Assert.That(result.BooksWithPublisherName.First().Title, Is.EqualTo("Test Book"));
        }

        [Test]
        public async Task GetAuthorDetailsByIdAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetAuthorByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            var result = await _sut.GetAuthorDetailsByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetEmptyAuthorViewModelAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public void GetEmptyAuthorViewModelAsync_ReturnsNewEmptyDto()
        {
            var result = _sut.GetEmptyAuthorViewModelAsync();

            Assert.That(result, Is.Not.Null);
            Assert.That(result, Is.InstanceOf<AuthorsAllDto>());
        }

        // ──────────────────────────────────────────────────────────────────────
        // AddNewAuthorAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task AddNewAuthorAsync_ValidModel_CallsRepositoryOnce()
        {
            var dto = new AuthorsAllDto { FullName = "  New Author  " };
            _repoMock.Setup(r => r.AddAuthorAsync(It.IsAny<Author>())).Returns(Task.CompletedTask);

            await _sut.AddNewAuthorAsync(dto);

            _repoMock.Verify(r => r.AddAuthorAsync(It.Is<Author>(a => a.FullName == "New Author")), Times.Once);
        }

        [Test]
        public void AddNewAuthorAsync_RepositoryThrowsDbUpdateException_ThrowsAuthorCreateException()
        {
            var dto = new AuthorsAllDto { FullName = "New Author" };
            _repoMock.Setup(r => r.AddAuthorAsync(It.IsAny<Author>()))
                     .ThrowsAsync(new DbUpdateException());

            Assert.ThrowsAsync<AuthorCreateException>(() => _sut.AddNewAuthorAsync(dto));
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetNewAuthorForEditByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetNewAuthorForEditByIdAsync_ExistingId_ReturnsDto()
        {
            var id = Guid.NewGuid();
            var author = new Author { Id = id, FullName = "Edit Me" };
            _repoMock.Setup(r => r.GetAuthorForEditByIdAsync(id)).ReturnsAsync(author);

            var result = await _sut.GetNewAuthorForEditByIdAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.FullName, Is.EqualTo("Edit Me"));
        }

        [Test]
        public async Task GetNewAuthorForEditByIdAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetAuthorForEditByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            var result = await _sut.GetNewAuthorForEditByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // UpdateNewAuthorAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task UpdateNewAuthorAsync_ExistingId_CallsRepositoryAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            var author = new Author { Id = id, FullName = "Old Name" };
            var dto = new AuthorsAllDto { Id = id, FullName = "New Name" };

            _repoMock.Setup(r => r.GetAuthorForEditByIdAsync(id)).ReturnsAsync(author);
            _repoMock.Setup(r => r.UpdateAuthorAsync(id, It.IsAny<Author>())).ReturnsAsync(true);

            var result = await _sut.UpdateNewAuthorAsync(id, dto);

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.UpdateAuthorAsync(id, It.Is<Author>(a => a.FullName == "New Name")), Times.Once);
        }

        [Test]
        public async Task UpdateNewAuthorAsync_NonExistingId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetAuthorForEditByIdAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            var result = await _sut.UpdateNewAuthorAsync(Guid.NewGuid(), new AuthorsAllDto { FullName = "X" });

            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.UpdateAuthorAsync(It.IsAny<Guid>(), It.IsAny<Author>()), Times.Never);
        }

        // ──────────────────────────────────────────────────────────────────────
        // GetAuthorNewDeleteDetailsAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task GetAuthorNewDeleteDetailsAsync_ExistingId_ReturnsDeleteDto()
        {
            var id = Guid.NewGuid();
            var author = new Author { Id = id, FullName = "Delete Me" };
            _repoMock.Setup(r => r.GetAuthorDeleteDetailsAsync(id)).ReturnsAsync(author);

            var result = await _sut.GetAuthorNewDeleteDetailsAsync(id);

            Assert.That(result, Is.Not.Null);
            Assert.That(result!.Id, Is.EqualTo(id));
            Assert.That(result.FullName, Is.EqualTo("Delete Me"));
        }

        [Test]
        public async Task GetAuthorNewDeleteDetailsAsync_NonExistingId_ReturnsNull()
        {
            _repoMock.Setup(r => r.GetAuthorDeleteDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            var result = await _sut.GetAuthorNewDeleteDetailsAsync(Guid.NewGuid());

            Assert.That(result, Is.Null);
        }

        // ──────────────────────────────────────────────────────────────────────
        // DeleteAuthorByIdAsync
        // ──────────────────────────────────────────────────────────────────────

        [Test]
        public async Task DeleteAuthorByIdAsync_NonExistingId_ReturnsFalse()
        {
            _repoMock.Setup(r => r.GetAuthorDeleteDetailsAsync(It.IsAny<Guid>())).ReturnsAsync((Author?)null);

            var result = await _sut.DeleteAuthorByIdAsync(Guid.NewGuid());

            Assert.That(result, Is.False);
            _repoMock.Verify(r => r.DeleteAuthorAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task DeleteAuthorByIdAsync_AuthorWithBooks_ThrowsAuthorDeleteException()
        {
            var id = Guid.NewGuid();
            var author = new Author
            {
                Id = id,
                FullName = "Has Books",
                BooksAuthors = new List<BookAuthor>
                {
                    new BookAuthor { BookId = Guid.NewGuid(), AuthorId = id, Book = null!, Author = null! }
                }
            };
            _repoMock.Setup(r => r.GetAuthorDeleteDetailsAsync(id)).ReturnsAsync(author);

            Assert.ThrowsAsync<AuthorDeleteException>(() => _sut.DeleteAuthorByIdAsync(id));
            _repoMock.Verify(r => r.DeleteAuthorAsync(It.IsAny<Guid>()), Times.Never);
        }

        [Test]
        public async Task DeleteAuthorByIdAsync_AuthorWithNoBooks_CallsRepositoryAndReturnsTrue()
        {
            var id = Guid.NewGuid();
            var author = new Author { Id = id, FullName = "No Books", BooksAuthors = new List<BookAuthor>() };
            _repoMock.Setup(r => r.GetAuthorDeleteDetailsAsync(id)).ReturnsAsync(author);
            _repoMock.Setup(r => r.DeleteAuthorAsync(id)).ReturnsAsync(true);

            var result = await _sut.DeleteAuthorByIdAsync(id);

            Assert.That(result, Is.True);
            _repoMock.Verify(r => r.DeleteAuthorAsync(id), Times.Once);
        }
    }
}
