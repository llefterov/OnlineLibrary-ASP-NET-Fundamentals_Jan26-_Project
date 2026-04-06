using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text;

namespace OnlineLibrary.Data.Models
{
    [PrimaryKey(nameof(UserId), nameof(BookId))]
    public class UserBook
    {
        [ForeignKey(nameof(User))]
        public Guid UserId { get; set; }

        public virtual ApplicationUser User { get; set; } = null!;

        [ForeignKey(nameof(Book))]
        public Guid BookId { get; set; }
        public virtual Book Book { get; set; } = null!;

        public bool IsRead { get; set; } = false;

        public DateTime? DateRead { get; set; }
    }
}
