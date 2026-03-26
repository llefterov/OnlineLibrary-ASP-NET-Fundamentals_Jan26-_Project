using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.GCommon.Exceptions.PublisherExceptions;

namespace OnlineLibrary.Data.Repository
{
    public class PublisherRepository: BaseRepository, IPublisherRepository
    {
        public PublisherRepository(OnlineLibraryDbContext dbContext)
            : base(dbContext)
        {
        }
        public async Task<IEnumerable<Publisher>> GetAllPublishersAsync()
        {
            var publishers = await DbContext.Publishers
                .OrderBy(p => p.Name)
                .Select(p => new Publisher
                {
                    Id = p.Id,
                    Name = p.Name
                })
                .ToListAsync();

            return publishers;
        }

        public async Task<Publisher?> GetPublisherByIdAsync(Guid id)
        {
            var publisher = await DbContext.Publishers
               .Include(p => p.Books)
               .ThenInclude(b => b.BooksAuthors)
               .ThenInclude(ba => ba.Author)
               .AsNoTracking()
               .Where(p => p.Id == id)
               .FirstOrDefaultAsync();

            return publisher;
        }

        public Task<Publisher> GetEmptyPublisherFormModelAsync()
        {
            Publisher emptyAuthorFormModel = new Publisher();
            return Task.FromResult(emptyAuthorFormModel);
        }

        public async Task AddPublisherAsync(Publisher inputModel)
        {
            var normalizedName = inputModel.Name.Trim();
            var normalizedNameUpper = normalizedName.ToUpper();

            var publisher = new Publisher
            {
                Name = normalizedName
            };

            if (await DbContext.Publishers.AnyAsync(p => p.Name.ToUpper() == normalizedNameUpper))
            {
                throw new PublisherAlreadyExistsException(publisher.Name);
            }

            await DbContext.Publishers.AddAsync(publisher);

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                if (dbEx.InnerException?.Message.Contains("IX_Publishers_Name", StringComparison.OrdinalIgnoreCase) == true)
                {
                    throw new PublisherAlreadyExistsException(publisher.Name);
                }

                throw new PublisherCreateException("Unable to save the publisher to the database.", dbEx);
            }
        }

        public async Task<Publisher?> GetPublisherForEditByIdAsync(Guid id)
        {
            var publisher = await DbContext.Publishers.FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return null;
            }

            var inputModel = new Publisher
            {
                Id = publisher.Id,
                Name = publisher.Name
            };
            return inputModel;
        }

        public async Task<bool> ExistsAsync(Guid id)
        {
            bool publisherExist = await DbContext
                .Publishers
                .AnyAsync(a => a.Id == id);

            return publisherExist;
        }

        public async Task<bool> UpdatePublisherAsync(Guid id, Publisher model)
        {
            // Ensure that the route id and the model id (if provided) are consistent
            if (model.Id != Guid.Empty && model.Id != id)
            {
                throw new PublisherUpdateExeption("Publisher ID mismatch between route and payload.");
            }

            var publisher = await DbContext.Publishers
               .FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {
                return false;
            }

            publisher.Name = model.Name;

            try
            {
                DbContext.Publishers.Update(publisher);
                await DbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new PublisherUpdateExeption("Unable to update the publisher in the database.");
            }
        }

        public async Task<Publisher?> GetPublisherDeleteDetailsAsync(Guid id)
        {
            var publisherToDelete = await DbContext.Publishers
                .Include(b => b.Books)
                .Select(b => new Publisher
                {
                    Id = b.Id,
                    Name = b.Name,
                    Books = b.Books
                })
                .FirstOrDefaultAsync(b => b.Id == id);

            return publisherToDelete;
        }

        public async Task<bool> DeletePublisherAsync(Guid id)
        {
            var publisher = await DbContext.Publishers
                .Include(b => b.Books)
                .FirstOrDefaultAsync(b => b.Id == id);

            if (publisher == null)
            {
                return false;
            }

            if (publisher.Books.Any())
            {
                throw new PublisherDeleteException("Cannot delete publisher with associated books.");
            }

            DbContext.Publishers.Remove(publisher);

            try
            {
                await DbContext.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateException)
            {
                throw new PublisherDeleteException("Unable to delete the publisher from the database.");
            }
        }
    }
}
