using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Publisher
{
    using OnlineLibrary.Data.Models;
    public class PublisherDeleteViewModel
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
