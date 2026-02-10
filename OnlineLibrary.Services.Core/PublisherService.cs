using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data;
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



        public async Task<IEnumerable<PublisherAllViewModel>> GetAllAsync()
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

        public async Task<PublisherDetailsViewModel?> GetByIdAsync(int id)
        {
            var publisher = await dbContext.Publishers
               .Include(p => p.Books)
               .Select(p => new PublisherDetailsViewModel
               {
                   Id = p.Id,
                   Name = p.Name,
                   Books = p.Books
               })
               .FirstOrDefaultAsync(a => a.Id == id);

            return publisher;
        }
    }
}
