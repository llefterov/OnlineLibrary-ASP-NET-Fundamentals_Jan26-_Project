using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data;

public class OnlineLibraryDbContext : IdentityDbContext<IdentityUser>
{
    public OnlineLibraryDbContext(DbContextOptions<OnlineLibraryDbContext> options)
        : base(options)
    {
    }

    public virtual DbSet<Book> Books { get; set; } = null!;
    public virtual DbSet<Author> Authors { get; set; } = null!;
    public virtual DbSet<Publisher> Publishers { get; set; } = null!;
    public virtual DbSet<BookAuthor> BooksAuthors { get; set; } = null!;



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);
        // Customize the ASP.NET Identity model and override the defaults if needed.
        // For example, you can rename the ASP.NET Identity table names and more.
        // Add your customizations after calling base.OnModelCreating(builder);

        builder.ApplyConfigurationsFromAssembly(typeof(OnlineLibraryDbContext).Assembly);
    }
}
