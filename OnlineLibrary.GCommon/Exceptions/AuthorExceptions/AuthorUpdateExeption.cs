using System;
using System.Collections.Generic;
using System.Text;

namespace OnlineLibrary.GCommon.Exceptions.AuthorExceptions
{
    public class AuthorUpdateExeption :Exception
    {
        public AuthorUpdateExeption(string fullName): base($"An error occurred while updating the author with the name '{fullName}'. Please try again.")
        {
            
        }
    }
}
