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
                new Author { Id = 1, FullName = "Jane Austen" },
               new Author { Id = 2, FullName = "George Orwell" },
               new Author { Id = 3, FullName = "Isaac Asimov" },
               new Author { Id = 4, FullName = "R.R. Tolkien" },
               new Author { Id = 5, FullName = "Agatha Christie" });
        }
    }
}
