using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Author
{
    public class AuthorDeleteViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = null!;
        public virtual ICollection<BookAuthor> BooksAuthors { get; set; } = new HashSet<BookAuthor>();
    }
}
