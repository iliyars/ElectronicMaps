using ElectronicMaps.Application.DTOs.Components;


namespace ElectronicMaps.Application.Abstractions.Queries.Components
{
    public interface IComponentReadRepository
    {
        Task<ComponentLookUpDto?> GetByIdAsync(int id, CancellationToken ct);
        Task<ComponentLookUpDto?> GetByNameAsync(string name, CancellationToken ct);
        // TODO: Create DTO
        Task<IReadOnlyList<ComponentLookUpDto>> GetAllAsync(CancellationToken ct);

        Task<IReadOnlyList<ComponentLookUpDto>> GetByFormCodeAsync(string formCode, CancellationToken ct);
        Task<IReadOnlyList<ComponentLookUpDto>> GetLookupByNamesAsync(IEnumerable<string> names, CancellationToken ct);

        Task<bool> ExistsAsync(string name, CancellationToken ct);


    }
}
