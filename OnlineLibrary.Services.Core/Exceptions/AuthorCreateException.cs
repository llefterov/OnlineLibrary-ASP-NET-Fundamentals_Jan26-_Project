using System;

namespace OnlineLibrary.Services.Core.Exceptions
{
    public class AuthorCreateException : Exception
    {
        public AuthorCreateException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}