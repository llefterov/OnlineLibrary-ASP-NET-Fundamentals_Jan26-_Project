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
                new BookAuthor { BookId = Guid.Parse("f0c604df-a030-437f-9028-0ada33e35b85"), AuthorId = 1 }, // Pride and Prejudice - Jane Austen
                new BookAuthor { BookId = Guid.Parse("2dc0a369-6c0d-44a7-a7b0-41959009d322"), AuthorId = 2 }, // 1984 - George Orwell
                new BookAuthor { BookId = Guid.Parse("c697d648-8fc0-41cb-9fb1-105792262850"), AuthorId = 3 }, // Foundation - Isaac Asimov
                new BookAuthor { BookId = Guid.Parse("23c5dbca-dba7-46ff-ae96-7b233a8ca88c"), AuthorId = 4 }, // The Hobbit - Agatha Christie
                new BookAuthor { BookId = Guid.Parse("1411eab8-b839-441d-a72d-2bb3cf7aa218"), AuthorId = 5 }  // Harry Potter - J.K. Rowling
            );
        }


    }
}
