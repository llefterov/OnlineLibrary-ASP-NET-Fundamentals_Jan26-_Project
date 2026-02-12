using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Author
{
    public class AuthorDetailsViewModel
    {
        public int Id { get; set; }
        public string FullName { get; set; } = null!;

        public ICollection<AuthorBookViewModel> BooksWithPublisherName { get; set; } = new HashSet<AuthorBookViewModel>();
    }
}
