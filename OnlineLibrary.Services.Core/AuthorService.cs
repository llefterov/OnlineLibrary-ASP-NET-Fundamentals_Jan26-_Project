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

        //public async Task<bool> ExistsAsync(Guid id)
        //{
        //    bool authorExist = await dbContext
        //        .Authors
        //        .AnyAsync(a => a.Id == id);

        //    return authorExist;
        //}

        public AuthorsAllDto GetEmptyAuthorViewModelAsync()
        {
            AuthorsAllDto emptyAuthorViewModel = new AuthorsAllDto();
            return emptyAuthorViewModel;
        }

        public async Task AddNewAuthorAsync(AuthorsAllDto inputModelDto)
        {
            var author = new Author
            {
                FullName = inputModelDto.FullName
            }
            ;

            if ((await authorRepository.GetAllAuthorsAsync()).Any(a => a.FullName == author.FullName))
            {
                throw new AuthorAlreadyExistsException(author.FullName);
            }

            try
            {
                await authorRepository.AddAuthorAsync(author);

                //await authorRepository.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new AuthorCreateException("Unable to save the author to the database.", dbEx);
            }
        }



        //public async Task<bool> ExistsAsync(Guid id)
        //{
        //    bool authorExist = await dbContext
        //        .Authors
        //        .AnyAsync(a => a.Id == id);

        //    return authorExist;
        //}


        public async Task<AuthorsAllDto> GetNewAuthorForEditByIdAsync(Guid id)
        {


            if (!(await authorRepository.ExistsAsync(id)))
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            var author = await authorRepository.GetAuthorForEditByIdAsync(id);

            if (author == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            var inputModel = new AuthorsAllDto
            {
                Id = author.Id,
                FullName = author.FullName
            };
            return inputModel;
        }

        public async Task UpdateNewAuthorAsync(Guid id, AuthorsAllDto model)
        {
            var author = await authorRepository.GetAuthorForEditByIdAsync(id);

            if (author == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            author.FullName = model.FullName;

            try
            {
                await authorRepository.UpdateAuthorAsync(id, author);
            }
            catch (DbUpdateException)
            {
                throw new AuthorUpdateExeption("Unable to update the author in the database.");
            }

        }

        //public async Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(Guid id)
        //{
        //    var authorToDelete = await dbContext.Authors
        //        .Include(a => a.BooksAuthors)
        //        .ThenInclude(ba => ba.Book) 
        //        .Select(a => new AuthorDeleteViewModel
        //        {
        //            Id = a.Id,
        //            FullName = a.FullName,
        //            BooksAuthors = a.BooksAuthors
        //        })
        //        .FirstOrDefaultAsync(a => a.Id == id);

        //    if (authorToDelete == null)
        //    {
        //        throw new AuthorDoesntExistException("Author not found.");
        //    }
        //    return authorToDelete;

        //}

        //public async Task DeleteAuthorAsync(Guid id)
        //{
        //    var author = await dbContext.Authors
        //        .Include(a => a.BooksAuthors)
        //        .ThenInclude(ba => ba.Book)
        //        .FirstOrDefaultAsync(a => a.Id == id);

        //    var inputModel = await GetAuthorDeleteDetailsAsync(id);



        //    if (author == null)
        //    {
        //        throw new AuthorDoesntExistException("Author not found.");
        //    }

        //    if (author.BooksAuthors.Any())
        //    {

        //        throw new AuthorDeleteException("Cannot delete author with associated books.");

        //    }


        //    dbContext.Authors.Remove(author);

        //    try
        //    {
        //        dbContext.SaveChanges();
        //    }
        //    catch (DbUpdateException)
        //    {
        //        throw new AuthorDeleteException("Unable to delete the author from the database.");
        //    }
        //}
    }
}

