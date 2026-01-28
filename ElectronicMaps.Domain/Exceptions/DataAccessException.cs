using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Exceptions
{
    public class DataAccessException : DomainException
    {
        public DataAccessException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
