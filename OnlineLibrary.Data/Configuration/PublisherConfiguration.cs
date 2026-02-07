using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using OnlineLibrary.Data.Models;
using System;
using System.Collections.Generic;
using System.Text;
using static OnlineLibrary.GCommon.ValidationConstants;

namespace OnlineLibrary.Data.Configuration
{
    public class PublisherConfiguration : IEntityTypeConfiguration<Publisher>
    {
        public void Configure(EntityTypeBuilder<Publisher> builder)
        {
            // Property constraints consistent with ValidationConstants / model attributes
            builder.Property(p => p.Name)
                   .IsRequired()
                   .HasMaxLength(PublisherNameMaxLength);

            // Seed data for publishers
            builder.HasData(
                new Publisher { Id = 1, Name = "Apress" },
                new Publisher { Id = 2, Name = "Manning Publications" },
                new Publisher { Id = 3, Name = "O'Reilly Media" },
                new Publisher { Id = 4, Name = "Packt Publishing" },
                new Publisher { Id = 5, Name = "Addison-Wesley" }
            );
        }
    }
}
