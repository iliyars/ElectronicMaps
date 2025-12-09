using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    public enum StoreChangeKind
    {
        Unknown = 0,

        /// <summary>
        /// Добавлен новый компонент или обновлён существующий.
        /// </summary>
        Upserted,

        /// <summary>
        /// Компонент удалён.
        /// </summary>
        Removed,

        /// <summary>
        /// Хранилище полностью очищено.
        /// </summary>
        Cleared,

        /// <summary>
        /// Хранилище полностью заменено новым набором (ReplaceAll).
        /// </summary>
        Replaced,

        /// <summary>
        /// Данные загружены из файла (LoadAsync).
        /// </summary>
        Loaded,

        /// <summary>
        /// Данные сохранены в файл (SaveAsync).
        /// </summary>
        Saved
    }
}
