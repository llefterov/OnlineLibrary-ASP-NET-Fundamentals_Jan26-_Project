using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Books
{
    public class BookFavoritesViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string CoverUrl { get; set; } = null!;

        public bool IsRead { get; set; }

        public string? DateRead { get; set; }
    }
}
