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

        public Author GetEmptyAuthorFormModelAsync()
        {
            Author emptyAuthorModel = new Author();
            return emptyAuthorModel;
        }

        public async Task AddAuthorAsync(Author inputModel)
        {
            var author = new Author
            {
                FullName = inputModel.FullName
            };

            if (await DbContext.Authors.AnyAsync(a => a.FullName == author.FullName))
            {
                throw new InvalidOperationException($"Author with name '{author.FullName}' already exists.");
            }

            await DbContext.Authors.AddAsync(author);

            try
            {
                await DbContext.SaveChangesAsync();
            }
            catch (DbUpdateException dbEx)
            {
                throw new InvalidOperationException("Unable to save the author to the database.", dbEx);
            }
        }


    }
}
