using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    public class StoreChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Тип произошедшего изменения.
        /// </summary>
        public StoreChangeKind Kind { get; }

        /// <summary>
        /// Ключ изменённого элемента (обычно CanonicalName или CleanName).
        /// Null для массовых операций (ReplacedAll, Cleared, Loaded).
        /// </summary>
        public string? Key { get; }

        /// <summary>
        /// Текущее количество элементов в Store после изменения.
        /// </summary>
        public int Count { get; }

        public StoreChangedEventArgs(StoreChangeKind kind, string? key, int count)
        {
            Kind = kind;
            Key = key;
            Count = count;
        }
    }
}
