using ElectronicMaps.Domain.ValueObjects;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Domain.Services
{
    public interface IComponentNameParser
    {
        ParsedComponentName Parse(string rawName);
    }
}
