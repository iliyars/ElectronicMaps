using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Exceptions
{
    public class InvalidComponentNameException : DomainException
    {
        public string? InvalidName { get; }

        public InvalidComponentNameException(string message)
            : base(message)
        {
        }
        public InvalidComponentNameException(string message, string invalidName)
            : base(message)
        {
            InvalidName = invalidName;
        }
    }
}
