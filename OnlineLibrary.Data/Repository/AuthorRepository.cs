using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;

namespace OnlineLibrary.Data.Repository
{
    public class AuthorRepository : BaseRepository, IAuthorRepository
    {
        public AuthorRepository(OnlineLibraryDbContext dbContext)
            : base(dbContext)
        {
        }

        public async Task<IEnumerable<Author>> GetAllAuthorsAsync()
        {
            var authors = await DbContext.Authors
             .OrderBy(a => a.FullName)
             .Select(a => new Author
             {
                 Id = a.Id,
                 FullName = a.FullName
             })
             .ToListAsync();

            return authors;
        }

        public async Task<Author?> GetAuthorByIdAsync(Guid id)
        {
            var author = await DbContext.Authors
                .Include(a => a.BooksAuthors)
                .ThenInclude(ba => ba.Book)
                .ThenInclude(b => b.Publisher)
                .AsNoTracking()
                .Where(a => a.Id == id)
                .FirstOrDefaultAsync();

            return author;
        }
    }
}
