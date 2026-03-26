using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Services.Models.Author;
using System.Globalization;
using static OnlineLibrary.GCommon.ApplicationConstants;

namespace OnlineLibrary.Services.Core
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository authorRepository;
        public AuthorService(IAuthorRepository authorRepository)
        {
            this.authorRepository = authorRepository;
        }

        public async Task<IEnumerable<AuthorsAllDto>> GetAllAuthorsForViewModelAsync()
        {
            var authorsData = await authorRepository.GetAllAuthorsAsync();

            var authors = authorsData
                .Select(a => new AuthorsAllDto
                {
                    Id = a.Id,
                    FullName = a.FullName
                })
                .ToList();

            return authors;
        }

        public async Task<AuthorDetailsDto?> GetAuthorDetailsByIdAsync(Guid id)
        {
            var authorData = await authorRepository.GetAuthorByIdAsync(id);

            if (authorData == null)
            {
                return null;
            }

            AuthorDetailsDto? authorDto = new AuthorDetailsDto
            {
                Id = authorData.Id,
                FullName = authorData.FullName,
                BooksWithPublisherName = authorData.BooksAuthors
                    .OrderBy(ba => ba.Book.Title)
                    .Select(ba => new AuthorBookDto
                    {
                        Id = ba.Book.Id,
                        Title = ba.Book.Title,
                        CoverUrl = ba.Book.CoverUrl ?? string.Empty,
                        Rating = ba.Book.Rating,
                        DateAdded = ba.Book.DateAdded.ToString(DateTimeFormat, CultureInfo.InvariantCulture),
                        GenreName = ba.Book.Genre.ToString(),
                        PublisherName = ba.Book.Publisher != null ? ba.Book.Publisher.Name : string.Empty,
                        Description = ba.Book.Description
                    })
                    .ToList()
            };

            return authorDto;
        }

        public AuthorsAllDto GetEmptyAuthorViewModelAsync()
        {
            AuthorsAllDto emptyAuthorViewModel = new AuthorsAllDto();
            return emptyAuthorViewModel;
        }

        public async Task AddNewAuthorAsync(AuthorsAllDto inputModelDto)
        {
            var normalizedFullName = inputModelDto.FullName.Trim();

            var author = new Author
            {
                FullName = normalizedFullName
            };

            try
            {
                await authorRepository.AddAuthorAsync(author);
            }
            catch (DbUpdateException dbEx)
            {
                throw new AuthorCreateException("Unable to save the author to the database.", dbEx);
            }
        }


        public async Task<AuthorsAllDto?> GetNewAuthorForEditByIdAsync(Guid id)
        {
            var author = await authorRepository.GetAuthorForEditByIdAsync(id);

            if (author == null)
            {
                return null;
            }

            var inputModel = new AuthorsAllDto
            {
                Id = author.Id,
                FullName = author.FullName
            };
            return inputModel;
        }

        public async Task<bool> UpdateNewAuthorAsync(Guid id, AuthorsAllDto model)
        {
            var author = await authorRepository.GetAuthorForEditByIdAsync(id);

            if (author == null)
            {
                return false;
            }

            author.FullName = model.FullName;

            return await authorRepository.UpdateAuthorAsync(id, author);
        }

        public async Task<AuthorDeleteDto?> GetAuthorNewDeleteDetailsAsync(Guid id)
        {
            Author? authorToDelete = await authorRepository.GetAuthorDeleteDetailsAsync(id);

            if (authorToDelete == null)
            {
                return null;
            }

            var authorToDeleteDto = new AuthorDeleteDto
            {
                Id = authorToDelete.Id,
                FullName = authorToDelete.FullName,
                BooksAuthors = authorToDelete.BooksAuthors
            };

            return authorToDeleteDto;
        }

        public async Task<bool> DeleteAuthorByIdAsync(Guid id)
        {
            Author? author = await authorRepository.GetAuthorDeleteDetailsAsync(id);

            if (author == null)
            {
                return false;
            }

            if (author.BooksAuthors.Any())
            {
                throw new AuthorDeleteException("Cannot delete author with associated books.");
            }

            return await authorRepository.DeleteAuthorAsync(id);
        }
    }
}

