using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Core.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;
using System.Linq;
using System.Globalization;
using static OnlineLibrary.GCommon.ApplicationConstants;

namespace OnlineLibrary.Services.Core
{
    public class AuthorService : IAuthorService
    {
        private readonly OnlineLibraryDbContext dbContext;
        public AuthorService(OnlineLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }

        public async Task<IEnumerable<AuthorAllViewModel>> GetAllAuthorsAsync()
        {
            var authors = await dbContext.Authors
                .OrderBy(a => a.FullName)
                .Select(a => new AuthorAllViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName
                })
                .ToListAsync();

            return authors;
        }

        public async Task<AuthorDetailsViewModel?> GetAuthorByIdAsync(int id)
        {
            var author = await dbContext.Authors
                .Include(a => a.BooksAuthors)
                .ThenInclude(ba => ba.Book)
                .ThenInclude(b => b.Publisher)
                .AsNoTracking()
                .Where(a => a.Id == id)
                .Select(a => new AuthorDetailsViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    BooksWithPublisherName = a.BooksAuthors
                    .OrderBy(ba => ba.Book.Title)
                    .Select(ba => new AuthorBookViewModel
                    {
                        Id = ba.Book.Id,
                        Title = ba.Book.Title,
                        CoverUrl = ba.Book.CoverUrl ?? string.Empty,
                        Rating = ba.Book.Rating,
                        DateAdded = ba.Book.DateAdded.ToString(DateTimeFormat,CultureInfo.InvariantCulture),
                        GenreName = ba.Book.Genre.ToString(),
                        PublisherName = ba.Book.Publisher != null ? ba.Book.Publisher.Name : string.Empty,
                        Description = ba.Book.Description
                    })
                    .ToList()
                })
                .FirstOrDefaultAsync();

            return author;
        }

        public async Task<bool> ExistsAsync(int id)
        {
            bool authorExist = await dbContext
                .Authors
                .AnyAsync(a => a.Id == id);

            return authorExist;
        }

        public AuthorAddViewModel GetEmtyAuthorFormModelAsync()
        {
            AuthorAddViewModel emptyAuthorFormModel = new AuthorAddViewModel();
            return emptyAuthorFormModel;
        }

        public async Task AddAuthorAsync(AuthorAddViewModel inputModel)
        {
            var author = new Author
            {
                FullName = inputModel.FullName
            };

            if (await dbContext.Authors.AnyAsync(a => a.FullName == author.FullName))
            {
                throw new AuthorAlreadyExistsException(author.FullName);
            }

            await dbContext.Authors.AddAsync(author);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new AuthorCreateException("Unable to save the author to the database.", dbEx);
            }
        }

        public async Task<AuthorEditViewModel> GetAuthorForEditByIdAsync(int id)
        {


            if (!(await ExistsAsync(id)))
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            var author = await dbContext.Authors.FirstOrDefaultAsync(a => a.Id == id);

            if (author == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            var inputModel = new AuthorEditViewModel
            {
                Id = author.Id,
                FullName = author.FullName
            };
            return inputModel;
        }

        public async Task UpdateAuthorAsync(int id, AuthorEditViewModel model)
        {
            var author = await dbContext.Authors
               .FirstOrDefaultAsync(a => a.Id == model.Id);

            if (author == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            author.FullName = model.FullName;

            try
            {
                dbContext.Authors.Update(author);
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new AuthorUpdateExeption("Unable to update the author in the database.");
            }

        }

        public async Task<AuthorDeleteViewModel> GetAuthorDeleteDetailsAsync(int id)
        {
            var authorToDelete = await dbContext.Authors
                .Include(a => a.BooksAuthors)
                .ThenInclude(ba => ba.Book) 
                .Select(a => new AuthorDeleteViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    BooksAuthors = a.BooksAuthors
                })
                .FirstOrDefaultAsync(a => a.Id == id);

            if (authorToDelete == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }
            return authorToDelete;

        }

        public async Task DeleteAuthorAsync(int id)
        {
            var author = await dbContext.Authors
                .Include(a => a.BooksAuthors)
                .ThenInclude(ba => ba.Book)
                .FirstOrDefaultAsync(a => a.Id == id);

            var inputModel = await GetAuthorDeleteDetailsAsync(id);



            if (author == null)
            {
                throw new AuthorDoesntExistException("Author not found.");
            }

            if (author.BooksAuthors.Any())
            {

                throw new AuthorDeleteException("Cannot delete author with associated books.");

            }


            dbContext.Authors.Remove(author);

            try
            {
                dbContext.SaveChanges();
            }
            catch (DbUpdateException)
            {
                throw new AuthorDeleteException("Unable to delete the author from the database.");
            }
        }
    }
}

