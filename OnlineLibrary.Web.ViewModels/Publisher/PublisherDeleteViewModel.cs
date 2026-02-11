using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Web.ViewModels.Publisher
{
    public class PublisherDeleteViewModel
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();

    }
}
