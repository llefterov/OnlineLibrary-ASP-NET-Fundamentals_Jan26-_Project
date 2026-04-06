using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Data.Models
{
    public class Publisher
    {
        [Key]
        public Guid Id { get; set; }

        [Required]
        [MaxLength(PublisherNameMaxLength)]
        public string Name { get; set; } = null!;

        [Required]
        public virtual ICollection<Book> Books { get; set; } = new List<Book>();
    }
}
