using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Data.Configuration
{
    public class AuthorConfiguration : IEntityTypeConfiguration<Author>
    {
        public void Configure(EntityTypeBuilder<Author> builder)
        {
            builder.HasData(
                new Author { Id = Guid.Parse("abebfdd2-7a9e-4aa7-84b8-454b6ac74f1d"), FullName = "Jane Austen" },
                new Author { Id = Guid.Parse("9a17ee9f-f2c6-44f4-9247-ec5fcbec4f92"), FullName = "George Orwell" },
                new Author { Id = Guid.Parse("2c5f78aa-7fca-46f7-a112-4db9d4c0d093"), FullName = "Isaac Asimov" },
                new Author { Id = Guid.Parse("fb1ac6db-3c36-4f35-bdce-8dcd4a61156e"), FullName = "R.R. Tolkien" },
                new Author { Id = Guid.Parse("f625bc9b-1b34-44fd-b88b-cbec15d529c6"), FullName = "Agatha Christie" });
        }
    }
}
