using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Books
{
    public class BookDeleteViewModel
    {
        public Guid Id { get; set; }

        public string Title { get; set; } = null!;

        public string? AddedByUserName { get; set; }

        public string? CoverUrl { get; set; }





    }
}
