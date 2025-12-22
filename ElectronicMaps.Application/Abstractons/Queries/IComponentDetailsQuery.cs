using ElectronicMaps.Application.DTO.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractons.Queries
{
    /// <summary>
    /// Запрос детальной информации о конкретном компоненте
    /// для пользовательского сценария работы с компонентами.
    ///
    /// Используется при просмотре и заполнении параметров
    /// отдельных компонентов (формы, отличные от формы 4).
    /// Возвращает параметры, относящиеся только к самому компоненту.
    ///
    /// Непроверенные компоненты также возвращаются — информация
    /// о статусе проверки передаётся в DTO и обрабатывается на уровне UI.
    /// </summary>
    public interface IComponentDetailsQuery
    {
        Task<ComponentDetailsDto> GetAsync(int componentId, CancellationToken ct);
    }
}
