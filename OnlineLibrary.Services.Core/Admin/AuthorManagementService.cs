using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.Services.Core.Admin.Interfaces;

namespace OnlineLibrary.Services.Core.Admin
{
    public class AuthorManagementService : AuthorService, IAuthorManagementService
    {
        public AuthorManagementService(IAuthorRepository authorRepository)
            : base(authorRepository)
        {
        }
    }
}
