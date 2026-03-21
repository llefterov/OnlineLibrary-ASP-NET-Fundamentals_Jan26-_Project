using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using OnlineLibrary.Data.Configuration;
using OnlineLibrary.Data.Models;

namespace OnlineLibrary.Data;

public class OnlineLibraryDbContext : IdentityDbContext<ApplicationUser, IdentityRole<Guid>, Guid>
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

        // Used a precomputed password hash to avoid dynamic changes in HasData
        var defaultUser = new ApplicationUser
        {
            Id = new Guid("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1"),
            UserName = "admin@onlinelibrary.com",
            NormalizedUserName = "ADMIN@ONLINELIBRARY.COM",
            Email = "admin@onlinelibrary.com",
            NormalizedEmail = "ADMIN@ONLINELIBRARY.COM",
            EmailConfirmed = true,
            // Precomputed hash for password "Admin123!" (stable across builds)
            PasswordHash = "AQAAAAIAAYagAAAAEHIaJlSfhU4K8tmJOH0vNC1Seaj11r0efwD90KrvKI+D5PDGPPUoibcXFsK6Gfh1Yg==",
            SecurityStamp = "3f5e9b60-7f08-4a84-9a2c-ec9d32f1cc7b",
            ConcurrencyStamp = "8a242145-60f2-4e35-8e5b-693a7d7bb6f4"
        };
        builder.Entity<ApplicationUser>().HasData(defaultUser);

        builder.ApplyConfigurationsFromAssembly(typeof(OnlineLibraryDbContext).Assembly);
    }
}
