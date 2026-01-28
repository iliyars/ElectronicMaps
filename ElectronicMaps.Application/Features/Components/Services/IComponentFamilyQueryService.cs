using ElectronicMaps.Application.DTOs.Families;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public interface IComponentFamilyQueryService
    {
        Task<ComponentFamilyLookupDto?> FindFamilyByNameAsync(
            string familyName, 
            CancellationToken ct = default);

        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllFamiliesAsync(
            CancellationToken ct = default);
    }
}
