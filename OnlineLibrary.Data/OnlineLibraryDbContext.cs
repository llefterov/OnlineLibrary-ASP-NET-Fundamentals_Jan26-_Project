using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace OnlineLibrary.Data
{
    public class OnlineLibraryDbContext(DbContextOptions<OnlineLibraryDbContext> options) : IdentityDbContext(options)
    {


    }
}
