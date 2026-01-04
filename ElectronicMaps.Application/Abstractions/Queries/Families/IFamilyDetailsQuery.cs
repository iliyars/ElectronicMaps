using ElectronicMaps.Application.DTOs.Families;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Queries.Families
{
    /// <summary>
    /// Запрос детальной информации о семействе компонентов
    /// для сценария работы с формой 4.
    ///
    /// Используется при сортировке и отображении компонентов,
    /// сгруппированных по семейству (форма 4).
    /// Возвращает параметры семейства и агрегированную информацию,
    /// необходимую для заполнения формы 4.
    ///
    /// Не предназначен для просмотра отдельных компонентов
    /// вне контекста формы 4.
    /// </summary>
    public interface IFamilyDetailsQuery
    {
        Task<FamilyDetailsDto?> GetAsync(int componentId, CancellationToken ct);
    }
}
