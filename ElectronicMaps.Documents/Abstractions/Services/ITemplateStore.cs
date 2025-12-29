using ElectronicMaps.Documents.Abstractions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Abstractions.Services
{
    public interface ITemplateStore
    {
        Stream OpenTemplate(TemplateId id);
    }
}
