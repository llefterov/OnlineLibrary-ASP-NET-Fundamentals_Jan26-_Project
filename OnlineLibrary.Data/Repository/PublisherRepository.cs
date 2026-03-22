using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;

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



    }
}
