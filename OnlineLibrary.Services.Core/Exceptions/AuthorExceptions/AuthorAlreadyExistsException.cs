using System;

namespace OnlineLibrary.Services.Core.Exceptions.AuthorExceptions
{
    public class AuthorAlreadyExistsException : Exception
    {
        public AuthorAlreadyExistsException(string fullName)
            : base($"An author with the name '{fullName}' already exists.")
        {
        }
    }
}