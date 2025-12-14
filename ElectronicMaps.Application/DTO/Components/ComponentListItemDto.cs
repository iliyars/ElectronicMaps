using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO.Components
{
    public record ComponentListItemDto(
        int Id,
        string Name,
        string FormCode,
        string? FamilyName
        );
}
