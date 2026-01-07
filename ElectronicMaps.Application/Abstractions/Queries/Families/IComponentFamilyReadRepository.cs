using ElectronicMaps.Application.DTOs.Families;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Families
{

    /// <summary>
    /// Репозиторий для чтения данных о семействах компонентов
    /// </summary>
    public interface IComponentFamilyReadRepository
    {
        /// <summary>
        /// Получить семейство по ID
        /// </summary>
        Task<ComponentFamilyLookupDto?> GetByIdAsync(int id, CancellationToken ct);
        /// <summary>
        /// Найти семейство по имени
        /// </summary>
        Task<ComponentFamilyLookupDto?> FindByNameAsync(string name, CancellationToken ct);

        /// <summary>
        /// Проверить существует ли семейство с указанным ID
        /// </summary>
        Task<bool> ExistsAsync(int id, CancellationToken ct = default);

        /// <summary>
        /// Проверить существует ли семейство с указанным именем
        /// </summary>
        Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);

        /// <summary>
        /// Получить все семейства
        /// </summary>
        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllAsync(CancellationToken ct = default);

        /// Получить семейства с пагинацией
        /// </summary>
        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetPagedAsync(
            int skip,
            int take,
            CancellationToken ct = default);

        /// <summary>
        /// Получить семейства по списку имён
        /// </summary>
        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetByNamesAsync(
            IEnumerable<string> names,
            CancellationToken ct = default);
    }
}
