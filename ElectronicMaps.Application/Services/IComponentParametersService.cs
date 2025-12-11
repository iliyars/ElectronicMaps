using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Services
{
    /// <summary>
    /// Сервис для получения параметров компонента из БД.
    /// </summary>
    public interface IComponentParametersService
    {
        /// <summary>
        /// Возвращает параметры семейства на основе его типа формы (FormType) и сохранённых значений.
        /// Параметры включают метаданные (ParameterDefinition) и значения (ParameterValue).
        /// </summary>
        /// <param name="familyId">Идентификатор компонентного семейства.</param>
        /// <param name="ct">Токен отмены.</param>
        Task<IReadOnlyCollection<ParameterDto>> GetFamilyParametersAsync(int familyId, CancellationToken ct);
        /// <summary>
        /// Возвращает параметры конкретного компонента на основе его типа формы (FormType)
        /// и сохранённых значений. Используется для показа индивидуальных характеристик элемента.
        /// </summary>
        /// <param name="componentId">Идентификатор компонента.</param>
        /// <param name="ct">Токен отмены.</param>
        Task<IReadOnlyCollection<ParameterDto>> GetComponentParametersAsync(int componentId, CancellationToken ct);
    }
}
