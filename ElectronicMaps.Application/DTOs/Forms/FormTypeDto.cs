using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Forms
{
    public record FormTypeDto(
        int Id,
        string Code,
        string Name
        );
}
