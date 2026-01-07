using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Families
{
    /// <summary>
    /// Информация о семействе для выбора в UI
    /// </summary>
    public record ComponentFamilyLookupDto
    {
        public required int Id { get; init; }
        public required string Name { get; init; }
        public int ComponentCount { get; init; }
        public int? FormTypeId { get; init; }
        public string? FormTypeName { get; init; }
        public string? FormCode { get; init; }
    }
}
