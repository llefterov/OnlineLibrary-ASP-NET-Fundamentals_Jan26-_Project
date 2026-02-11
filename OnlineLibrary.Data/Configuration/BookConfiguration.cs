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
            // Seed data for books
            builder.HasData(
                new Book
                {
                    Id = Guid.Parse("f0c604df-a030-437f-9028-0ada33e35b85"),
                    Title = "Pride and Prejudice",
                    Description = "A classic novel about love and society in early 19th-century England.",
                    Genre = BookGenre.Fiction,
                    IsRead = true,
                    DateRead = new DateTime(2020, 5, 1),
                    Rating = 5,
                    CoverUrl = "https://upload.wikimedia.org/wikipedia/en/0/03/Prideandprejudiceposter.jpg",
                    DateAdded = new DateTime(2020, 1, 15),
                    PublisherId = 1,

                },
                new Book
                {
                    Id = Guid.Parse("2dc0a369-6c0d-44a7-a7b0-41959009d322"),
                    Title = "1984",
                    Description = "Dystopian novel about surveillance and totalitarianism.",
                    Genre = BookGenre.ScienceFiction,
                    IsRead = true,
                    DateRead = new DateTime(2019, 8, 10),
                    Rating = 5,
                    CoverUrl = "https://m.media-amazon.com/images/I/612ADI+BVlL._AC_UF1000,1000_QL80_.jpg",
                    DateAdded = new DateTime(2019, 6, 20),
                    PublisherId = 2,
                },
                new Book
                {
                    Id = Guid.Parse("c697d648-8fc0-41cb-9fb1-105792262850"),
                    Title = "Foundation",
                    Description = "Epic science fiction series about the fall and rise of galactic empires.",
                    Genre = BookGenre.ScienceFiction,
                    IsRead = false,
                    DateRead = null,
                    Rating = 0,
                    CoverUrl = "https://cdn.mos.cms.futurecdn.net/oFCCtndaa9gxNqmJDY6Rp8.jpg",
                    DateAdded = new DateTime(2021, 3, 5),
                    PublisherId = 3,
                },
                new Book
                {
                    Id = Guid.Parse("23c5dbca-dba7-46ff-ae96-7b233a8ca88c"),
                    Title = "The Hobbit",
                    Description = "Fantasy adventure preceding the events of The Lord of the Rings.",
                    Genre = BookGenre.Fantasy,
                    IsRead = true,
                    DateRead = new DateTime(2018, 11, 2),
                    Rating = 5,
                    CoverUrl = "https://resizing.flixster.com/-XZAfHZM39UwaGJIFWKAE8fS0ak=/v3/t/assets/p9458059_p_v10_ac.jpg",
                    DateAdded = new DateTime(2018, 10, 12),
                    PublisherId = 4,
                },
                new Book
                {
                    Id = Guid.Parse("1411eab8-b839-441d-a72d-2bb3cf7aa218"),
                    Title = "Murder on the Orient Express",
                    Description = "Classic mystery featuring detective Hercule Poirot.",
                    Genre = BookGenre.Mystery,
                    IsRead = false,
                    DateRead = null,
                    Rating = 0,
                    CoverUrl = "https://www.blackcat-cideb.com/uploads/2020/02/COVER_Murder_on_the_orient_express_Agatha-Christie_f2a379ae1e65e577f341258edaba4148.jpg",
                    DateAdded = new DateTime(2022, 7, 1),
                    PublisherId = 5,
                }
            );
        }
    }
}
