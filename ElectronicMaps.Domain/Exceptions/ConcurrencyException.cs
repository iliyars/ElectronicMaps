using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Exceptions
{
    public class ConcurrencyException : DomainException
    {
        public ConcurrencyException(string message, Exception? innerException = null)
            : base(message, innerException)
        {
        }
    }
}
