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
                throw new PublisherCreateException("Unable to save the author to the database.", dbEx);
            }
        }

    }
}
