using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Domain.Exceptions
{
    public class ComponentNotFoundException : DomainException
    {
        public int ComponentId { get; }

        public ComponentNotFoundException(int componentId)
            : base($"Компонент с ID {componentId} не найден")
        {
            ComponentId = componentId;
        }

        public ComponentNotFoundException(string componentName)
            : base($"Компонент '{componentName}' не найден")
        {
        }
    }
}
