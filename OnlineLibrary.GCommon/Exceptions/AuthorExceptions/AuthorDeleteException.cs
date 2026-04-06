using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.GCommon.Exceptions.AuthorExceptions
{
    public class AuthorDeleteException : Exception
    {

        public AuthorDeleteException(string fullName) : base($"Cannot delete author '{fullName}' because they have associated books.")
        {

        }
    }
}

