using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Data.Models
{
    [PrimaryKey(nameof(UserId), nameof(BookId))]
    public class UserBook
    {
        public string UserId { get; set; } = null!;

        public virtual IdentityUser User { get; set; } = null!;

        public Guid BookId { get; set; }
        public virtual Book Book { get; set; } =  null!;

    }
}
