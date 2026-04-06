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
                new Publisher { Id = Guid.Parse("7b1cb8a1-cce4-4fe8-b258-a03e3468b761"), Name = "Apress" },
                new Publisher { Id = Guid.Parse("67ad1f17-8e47-47ce-be56-9d7bc2c09736"), Name = "Manning Publications" },
                new Publisher { Id = Guid.Parse("2da9d7ca-8f3f-4f0f-ab28-e84fc351338e"), Name = "O'Reilly Media" },
                new Publisher { Id = Guid.Parse("f2f4bc8f-b080-4624-8ba5-f64f0d006778"), Name = "Packt Publishing" },
                new Publisher { Id = Guid.Parse("e66f13fa-3f9b-4304-a450-7d0b907f22ec"), Name = "Addison-Wesley" }
            );
        }
    }
}
