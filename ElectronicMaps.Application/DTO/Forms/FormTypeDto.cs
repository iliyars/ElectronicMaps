using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Forms
{
    public record FormTypeDto(
        int Id,
        string Code,
        string Name
        );
}
