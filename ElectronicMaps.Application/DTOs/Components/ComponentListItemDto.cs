using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record ComponentListItemDto(
        int Id,
        string Name,
        string FormCode,
        string? FamilyName
        );
}
