using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Exceptions
{
    public class ValidationException : DomainException
    {
        public IReadOnlyList<string> Errors { get; set; }

        public ValidationException(string message)
            : base(message)
        {
            Errors = new List<string> { message };
        }

        public ValidationException(IEnumerable<string> errors)
            : base(string.Join("; ", errors))
        {
            Errors = errors.ToList();
        }

        public ValidationException(string message, IEnumerable<string> errors)
            : base(message)
        {
            Errors = errors.ToList();
        }

    }
}
