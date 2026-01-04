using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Remarks
{
    public record RemarkDto(
        int Id,
        string Text,
        int Order
    );
}
