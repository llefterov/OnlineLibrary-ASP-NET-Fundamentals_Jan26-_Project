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

        public Publisher GetEmptyPublisherFormModelAsync()
        {
            Publisher emptyAuthorFormModel = new Publisher();
            return emptyAuthorFormModel;
        }

        public async Task AddPublisherAsync(Publisher inputModel)
        {
            var publisher = new Publisher
            {
                Name = inputModel.Name
            };

            if (await DbContext.Publishers.AnyAsync(p => p.Name == publisher.Name))
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
                throw new PublisherCreateException("Unable to save the publisher to the database.", dbEx);
            }
        }

        public async Task<Publisher> GetPublisherForEditByIdAsync(Guid id)
        {


            if (!(await ExistsAsync(id)))
            {
                throw new PublisherDoesntExistException("Publisher not found.");
            }

            var publisher = await DbContext.Publishers.FirstOrDefaultAsync(p => p.Id == id);

            if (publisher == null)
            {

                throw new PublisherDoesntExistException("Publisher does not exist");

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

        public Task UpdatePublisherAsync(Guid id, Publisher model)
        {
            // Ensure that the route id and the model id (if provided) are consistent
            if (model.Id != Guid.Empty && model.Id != id)
            {
                throw new PublisherUpdateExeption("Publisher ID mismatch between route and payload.");
            }

            var publisher = DbContext.Publishers
               .FirstOrDefault(p => p.Id == id);

            if (publisher == null)
            {
                throw new PublisherDoesntExistException("Publisher does not exist");
            }

            publisher.Name = model.Name;

            try
            {
                DbContext.Publishers.Update(publisher);
                return DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new PublisherUpdateExeption("Unable to update the publisher in the database.");
            }
        }

        public async Task<Publisher> GetPublisherDeleteDetailsAsync(Guid id)
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

            if (publisherToDelete == null)
            {
                throw new PublisherDoesntExistException("Publisher does not exist");
            }

            return publisherToDelete;
        }

        public async Task DeletePublisherAsync(Guid id)
        {
            var publisher = await DbContext.Publishers
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

            DbContext.Publishers.Remove(publisher);

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException)
            {
                throw new PublisherDeleteException("Unable to delete the publisher from the database.");
            }
        }
    }
}
