using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Core.Exceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Author;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Threading.Tasks;

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
                .Where(a => a.Id == id)
                .Select(a => new AuthorDetailsViewModel
                {
                    Id = a.Id,
                    FullName = a.FullName,
                    BooksAuthors = a.BooksAuthors
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

            // business-level check -> throw domain-specific exception
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
                // wrap low-level exception to a service-level exception
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

            var inputModel = new AuthorEditViewModel
            {
                Id = author.Id,
                FullName = author.FullName
            };
            return inputModel;
        }

        public Task UpdateAuthorAsync(int id, AuthorEditViewModel model)
        {
            var author = dbContext.Authors
               .FirstOrDefault(a => a.Id == model.Id);

            author.FullName = model.FullName;

            try
            {
                dbContext.Authors.Update(author);
                return dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // wrap low-level exception to a service-level exception
                throw new AuthorUpdateExeption("Unable to update the author in the database.");
            }





        }
    } 
}





