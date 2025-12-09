using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Project
{
    public interface IProjectSaveService
    {
        /// <summary>
        /// Сохраняет текущий проект.
        /// Если путь ещё не задан, запрашивает его у пользователя (аналог Save As).
        /// Возвращает true, если проект сохранён, false если пользователь отменил.
        /// </summary>
        Task<bool> SaveCurrentProjectAsync(CancellationToken ct = default);

        /// <summary>
        /// Сохраняет проект как... Всегда показывает диалог выбора файла.
        /// </summary>
        Task<bool> SaveProjectAsAsync(CancellationToken ct = default);
    }
}
