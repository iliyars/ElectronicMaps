using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record ComponentLookupDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public required int FamilyId { get; init; }
        public string? FamilyName { get; init; }
        public int? FormTypeId { get; init; }
        public string? FormTypeName { get; init; }
        public string? FormCode { get; init; }
    }

}
