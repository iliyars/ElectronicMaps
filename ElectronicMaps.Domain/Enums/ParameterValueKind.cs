using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Enums
{
    public enum ParameterValueKind
    {
        String = 1,
        Int = 2,
        Double = 3,
        WithPins = 4 // значение + номера выводов
    }
}
