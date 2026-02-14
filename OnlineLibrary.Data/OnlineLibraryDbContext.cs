using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Configuration;
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
    public virtual DbSet<UserBook> UsersBooks { get; set; } = null!;



    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // Use a deterministic (precomputed) password hash to avoid dynamic changes in HasData
        var defaultUser = new IdentityUser
        {
            Id = "c8cb7abb-9a55-43ca-922c-2eac92b1e651",
            UserName = "admin@onlinelibrary.com",
            NormalizedUserName = "ADMIN@ONLINELIBRARY.COM",
            Email = "admin@onlinelibrary.com",
            NormalizedEmail = "ADMIN@ONLINELIBRARY.COM",
            EmailConfirmed = true,
            // Precomputed hash for password "Admin123!" (stable across builds)
            PasswordHash = "AQAAAAIAAYagAAAAEHIaJlSfhU4K8tmJOH0vNC1Seaj11r0efwD90KrvKI+D5PDGPPUoibcXFsK6Gfh1Yg==",
            SecurityStamp = "",
            ConcurrencyStamp = "b470e7b3-30bb-4b2c-aa33-7194da1a6e2d"
        };
        builder.Entity<IdentityUser>().HasData(defaultUser);


        builder.ApplyConfigurationsFromAssembly(typeof(OnlineLibraryDbContext).Assembly);
    }
}
