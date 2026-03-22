using System;

namespace OnlineLibrary.GCommon.Exceptions.AuthorExceptions
{
    public class AuthorCreateException : Exception
    {
        public AuthorCreateException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}