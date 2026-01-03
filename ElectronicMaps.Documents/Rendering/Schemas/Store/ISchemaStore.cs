using ElectronicMaps.Documents.Rendering.Schemas.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Documents.Rendering.Schemas.Store
{
    /// <summary>
    /// Интерфейс для хранилища схем форм
    /// </summary>
    public interface ISchemaStore
    {
        /// <summary>
        /// Получает схему формы по коду
        /// </summary>
        /// <param name="formCode">Код формы (например, "4", "5", "6")</param>
        /// <returns>Схема формы</returns>
        /// <exception cref="SchemaNotFoundException">Если схема не найдена</exception>
        public FormSchema GetSchema(string formCode);

        /// <summary>
        /// Проверяет наличие схемы для указанного кода формы
        /// </summary>
        /// <param name="formCode">Код формы</param>
        /// <returns>true, если схема существует</returns>
        bool HasSchema(string formCode);

        /// <summary>
        /// Получает список всех доступных кодов форм
        /// </summary>
        /// <returns>Коллекция кодов форм</returns>
        IReadOnlyCollection<string> GetAvailableFormCodes();

    }
}
