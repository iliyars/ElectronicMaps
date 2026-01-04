using ElectronicMaps.Application.DTOs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Workspace
{
    /// <summary>
    /// Запрос данных для пользовательского рабочего пространства (Workspace).
    /// 
    ///  Используется для отображения списка  рабочих компонентов,
    ///  их группировки и сортировки по формам.
    ///  Предназначен для пользовательского сценария работы с проектом,
    ///  а не для администрирования справочника.
    ///  
    ///  Возвращаемые данные оптимизированы для отображения списков и карточек
    ///   не содержат детальной информации о параметрах компонентов или семейств.s
    /// </summary>
    public interface IWorkspaceQuery
    {
        Task<IReadOnlyList<ComponentListItemDto>> GetAllComponentsAsync(CancellationToken ct);

        Task<IReadOnlyList<ComponentListItemDto>> GetComponentsByFormAsync(
            string formCode,
            CancellationToken ct);
    }
}
