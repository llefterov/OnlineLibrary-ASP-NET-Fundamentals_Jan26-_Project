using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Data.Configuration
{
    public class BookAuthorConfiguration : IEntityTypeConfiguration<BookAuthor>
    {
        public void Configure(EntityTypeBuilder<BookAuthor> builder)
        {

            // Seed data for BookAuthor relationships
            builder.HasData(
                new BookAuthor { BookId = Guid.Parse("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf"), AuthorId = Guid.Parse("abebfdd2-7a9e-4aa7-84b8-454b6ac74f1d") }, // Pride and Prejudice - Jane Austen
                new BookAuthor { BookId = Guid.Parse("0eea95e2-33be-4bf1-a851-84a040d4432a"), AuthorId = Guid.Parse("9a17ee9f-f2c6-44f4-9247-ec5fcbec4f92") }, // 1984 - George Orwell
                new BookAuthor { BookId = Guid.Parse("9f5ce95b-c0cd-4f1f-bff3-c3571d003319"), AuthorId = Guid.Parse("2c5f78aa-7fca-46f7-a112-4db9d4c0d093") }, // Foundation - Isaac Asimov
                new BookAuthor { BookId = Guid.Parse("6e916262-b412-4232-8e4d-5f822f9da185"), AuthorId = Guid.Parse("fb1ac6db-3c36-4f35-bdce-8dcd4a61156e") }, // The Hobbit - R.R. Tolkien
                new BookAuthor { BookId = Guid.Parse("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d"), AuthorId = Guid.Parse("f625bc9b-1b34-44fd-b88b-cbec15d529c6") }  // Murder on the Orient Express - Agatha Christie
            );
        }


    }
}
