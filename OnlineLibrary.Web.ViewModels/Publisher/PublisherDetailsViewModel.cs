using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Publisher
{
    public class PublisherDetailsViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;

        public ICollection<PublisherBookViewModel> BooksWithAuthorName { get; set; } = new List<PublisherBookViewModel>();
    }
}
