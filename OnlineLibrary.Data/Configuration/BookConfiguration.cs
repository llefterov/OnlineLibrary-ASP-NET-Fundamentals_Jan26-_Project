using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Data.Configuration
{
    public class BookConfiguration : IEntityTypeConfiguration<Book>
    {
        public void Configure(EntityTypeBuilder<Book> builder)
        {
            var adminUserId = Guid.Parse("d6f5de2f-4be6-4f7a-a3b5-c7c0d72ac8f1");

            // Seed data for books
            builder.HasData(
                new Book
                {
                    Id = Guid.Parse("e6f42e6f-6c1b-4b8f-bb2f-e87eb4fd8ecf"),
                    Title = "Pride and Prejudice",
                    Description = "A classic novel about love and society in early 19th-century England.",
                    Genre = BookGenre.Fiction,
                    Rating = 5,
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/0/03/Prideandprejudiceposter.jpg",
                    AddedByUserId = adminUserId,
                    DateAdded = new DateTime(2020, 1, 15),
                    PublisherId = Guid.Parse("7b1cb8a1-cce4-4fe8-b258-a03e3468b761"),

                },
                new Book
                {
                    Id = Guid.Parse("0eea95e2-33be-4bf1-a851-84a040d4432a"),
                    Title = "1984",
                    Description = "Dystopian novel about surveillance and totalitarianism.",
                    Genre = BookGenre.ScienceFiction,
                    Rating = 5,
                    CoverUrl = "https://m.media-amazon.com/images/I/612ADI+BVlL._AC_UF1000,1000_QL80_.jpg",
                    AddedByUserId = adminUserId,
                    DateAdded = new DateTime(2019, 6, 20),
                    PublisherId = Guid.Parse("67ad1f17-8e47-47ce-be56-9d7bc2c09736"),
                },
                new Book
                {
                    Id = Guid.Parse("9f5ce95b-c0cd-4f1f-bff3-c3571d003319"),
                    Title = "Foundation",
                    Description = "Epic science fiction series about the fall and rise of galactic empires.",
                    Genre = BookGenre.ScienceFiction,
                    Rating = 0,
                    CoverUrl = "https://cdn.mos.cms.futurecdn.net/oFCCtndaa9gxNqmJDY6Rp8.jpg",
                    AddedByUserId = adminUserId,
                    DateAdded = new DateTime(2021, 3, 5),
                    PublisherId = Guid.Parse("2da9d7ca-8f3f-4f0f-ab28-e84fc351338e"),
                },
                new Book
                {
                    Id = Guid.Parse("6e916262-b412-4232-8e4d-5f822f9da185"),
                    Title = "The Hobbit",
                    Description = "Fantasy adventure preceding the events of The Lord of the Rings.",
                    Genre = BookGenre.Fantasy,
                    Rating = 5,
                    CoverUrl = "https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p9458059_p_v10_ac.jpg",
                    AddedByUserId = adminUserId,
                    DateAdded = new DateTime(2018, 10, 12),
                    PublisherId = Guid.Parse("f2f4bc8f-b080-4624-8ba5-f64f0d006778"),
                },
                new Book
                {
                    Id = Guid.Parse("3a1508af-90c6-4eb0-bec6-f7b5ea096d2d"),
                    Title = "Murder on the Orient Express",
                    Description = "Classic mystery featuring detective Hercule Poirot.",
                    Genre = BookGenre.Mystery,
                    Rating = 0,
                    CoverUrl = "https://www.blackcat-cideb.com/uploads/2020/02/COVER_Murder_on_the_orient_express_Agatha-Christie_f2a379ae1e65e577f341258edaba4148.jpg",
                    AddedByUserId = adminUserId,
                    DateAdded = new DateTime(2022, 7, 1),
                    PublisherId = Guid.Parse("e66f13fa-3f9b-4304-a450-7d0b907f22ec"),
                }
            );
        }
    }
}
