using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Author
{
    public class AuthorAllViewModel
    {
        public int Id { get; set; }

        public string? FullName { get; set; }
    }
}
