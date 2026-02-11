using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.Services.Core.Exceptions.PublisherExceptions 
{
    public class PublisherUpdateExeption :Exception
    {
        public PublisherUpdateExeption(string Name): base($"An error occurred while updating the publisher with the name '{Name}'. Please try again.")
        {
            
        }
    }
}
