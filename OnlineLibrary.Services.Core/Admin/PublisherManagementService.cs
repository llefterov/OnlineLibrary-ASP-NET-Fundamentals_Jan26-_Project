using Microsoft.AspNetCore.Identity;
using OnlineLibrary.Data.Models;
using OnlineLibrary.Data.Repository.Contracts;
using OnlineLibrary.Services.Core.Admin.Interfaces;
using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Admin
{
    public class PublisherManagementService : PublisherService, IPublisherManagementService
    {
        public PublisherManagementService(IPublisherRepository publisherRepository)
            : base(publisherRepository)
        {
        }
    }
}
