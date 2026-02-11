using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Web.ViewModels.Publisher
{
    public class PublisherEditViewModel
    {
        [Required]
        public int Id { get; set; }

        [Required]
        [MinLength(PublisherNameMinLength)]
        [MaxLength(PublisherNameMaxLength)]
        public string Name { get; set; } = null!;
    }
}
