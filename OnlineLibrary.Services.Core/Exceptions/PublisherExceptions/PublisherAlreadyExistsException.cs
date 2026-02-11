using System;

namespace OnlineLibrary.Services.Core.Exceptions.PublisherExceptions
{
    public class PublisherAlreadyExistsException : Exception
    {
        public PublisherAlreadyExistsException(string Name)
            : base($"A Publisher with the name '{Name}' already exists.")
        {
        }
    }
}