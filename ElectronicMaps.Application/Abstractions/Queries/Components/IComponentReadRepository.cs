using ElectronicMaps.Application.DTOs.Components;


namespace ElectronicMaps.Application.Abstractions.Queries.Components
{
    /// <summary>
    /// Репозиторий для чтения данных о компонентах
    /// </summary>
    public interface IComponentReadRepository
    {
        /// <summary>
        /// Получить компонент по ID
        /// </summary>
        Task<ComponentLookupDto?> GetByIdAsync(int id, CancellationToken ct);

        Task<ComponentLookupDto?> FindByNameAsync(string name, CancellationToken ct);
        /// <summary>
        /// Проверить существует ли компонент с указанным именем
        /// </summary>
        Task<bool> ExistsAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// Получить все компоненты семейства
        /// </summary>
        Task<IReadOnlyList<ComponentLookupDto>> GetByFamilyIdAsync(
            int familyId,
            CancellationToken ct = default);

        /// <summary>
        /// Получить все компоненты
        /// </summary>
        Task<IReadOnlyList<ComponentLookupDto>> GetAllAsync(CancellationToken ct = default);

        /// <summary>
        /// Получить компоненты по списку имён
        /// </summary>
        Task<IReadOnlyList<ComponentLookupDto>> GetByNamesAsync(
            IEnumerable<string> names,
            CancellationToken ct = default);
    }
}
