using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Rendering.Schemas.Store
{
    public class SchemaNotFoundException : Exception
    {
        /// <summary>
        /// Код формы, для которой не найдена схема
        /// </summary>
        public string FormCode { get; }

        /// <summary>
        /// Создаёт исключение с кодом формы
        /// </summary>
        public SchemaNotFoundException(string formCode)
            : base($"Schema for form '{formCode}' not found")
        {
            FormCode = formCode;
        }

        /// <summary>
        /// Создаёт исключение с кастомным сообщением
        /// </summary>
        public SchemaNotFoundException(string formCode, string message)
            : base(message)
        {
            FormCode = formCode;
        }

        /// <summary>
        /// Создаёт исключение с внутренним исключением
        /// </summary>
        public SchemaNotFoundException(string formCode, string message, Exception innerException)
            : base(message, innerException)
        {
            FormCode = formCode;
        }
    }
}
