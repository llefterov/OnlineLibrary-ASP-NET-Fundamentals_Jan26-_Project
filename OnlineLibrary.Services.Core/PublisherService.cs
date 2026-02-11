using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Services.Core.Exceptions;
using OnlineLibrary.Services.Core.Exceptions.AuthorExceptions;
using OnlineLibrary.Services.Core.Exceptions.PublisherExceptions;
using OnlineLibrary.Services.Core.Interfaces;
using OnlineLibrary.Web.ViewModels.Author;
using OnlineLibrary.Web.ViewModels.Publisher;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core
{
    public class PublisherService : IPublisherService
    {
        private readonly OnlineLibraryDbContext dbContext;

        public PublisherService(OnlineLibraryDbContext dbContext)
        {
            this.dbContext = dbContext;
        }



        public async Task<IEnumerable<PublisherAllViewModel>> GetPublisherAllAsync()
        {
            var publishers = await dbContext.Publishers
            .OrderBy(p => p.Name)
            .Select(p => new PublisherAllViewModel
         {
             Id = p.Id,
             Name = p.Name
         })
         .ToListAsync();

            return publishers;
        }

        public async Task<PublisherDetailsViewModel?> GetPublisherByIdAsync(int id)
        {
            var publisher = await dbContext.Publishers
               .Include(p => p.Books)
               .Select(p => new PublisherDetailsViewModel
               {
                   Id = p.Id,
                   Name = p.Name,
                   Books = p.Books
               })
               .FirstOrDefaultAsync(p => p.Id == id);

            return publisher;
        }




        public PublisherAddViewModel GetEmtyPublisherFormModelAsync()
        {
            PublisherAddViewModel emptyAuthorFormModel = new PublisherAddViewModel();
            return emptyAuthorFormModel;
        }

        public async Task AddPublisherAsync(PublisherAddViewModel inputModel)
        {
            var publisher = new Publisher
            {
                Name = inputModel.Name
            };

            // business-level check -> throw domain-specific exception
            if (await dbContext.Publishers.AnyAsync(p => p.Name == publisher.Name))
            {
                throw new PublisherAlreadyExistsException(publisher.Name);
            }

            await dbContext.Publishers.AddAsync(publisher);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                // wrap low-level exception to a service-level exception
                throw new PublisherCreateException("Unable to save the author to the database.", dbEx);
            }
        }

        public async Task<PublisherEditViewModel> GetPublisherForEditByIdAsync(int id)
        {


            if (!(await ExistsAsync(id)))
            {
                throw new PublisherDoesntExistException("Publisher not found.");
            }

            var publisher = await dbContext.Publishers.FirstOrDefaultAsync(p => p.Id == id);

            var inputModel = new PublisherEditViewModel
            {
                Id = publisher.Id,
                Name = publisher.Name
            };
            return inputModel;
        }

        public Task UpdatePublisherAsync(int id, PublisherEditViewModel model)
        {
            var publisher = dbContext.Publishers
               .FirstOrDefault(p => p.Id == model.Id);

            publisher.Name = model.Name;

            try
            {
                dbContext.Publishers.Update(publisher);
                return dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // wrap low-level exception to a service-level exception
                throw new PublisherUpdateExeption("Unable to update the publisher in the database.");
            }

        }

        public async Task<bool> ExistsAsync(int id)
        {
            bool publisherExist = await dbContext
                .Publishers
                .AnyAsync(a => a.Id == id);

            return publisherExist;
        }


        public async Task<PublisherDeleteViewModel> GetPublisherDeleteDetailsAsync(int id)
        {
            var publisherToDelete = await dbContext.Publishers
                .Include(b => b.Books)
                .Select(b => new PublisherDeleteViewModel
                {
                    Id = b.Id,
                    Name = b.Name,
                    Books = b.Books
                })
                .FirstOrDefaultAsync(b  => b.Id == id);

            return publisherToDelete;

        }

        public async Task DeletePublisherAsync(int id)
        {
            var publisher = await dbContext.Publishers
                .Include(b => b.Books)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (publisher == null)
            {
                throw new PublisherDoesntExistException("Publisher not found.");
            }

            if (publisher.Books.Any())
            {

                throw new PublisherDeleteException("Cannot delete publisher with associated books.");

            }

            dbContext.Publishers.Remove(publisher);

            try
            {
                await dbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                // wrap low-level exception to a service-level exception
                throw new PublisherDeleteException("Unable to delete the publisher from the database.");
            }

        }



    }
}
