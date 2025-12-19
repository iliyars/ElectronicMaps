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

        public IReadOnlyList<Guid> DraftIds { get; }

        public int TotalWorkingCount { get; }

        public StoreChangedEventArgs(StoreChangeKind kind, string? key, int totalWorking, IReadOnlyList<Guid>? draftIds)
        {
            Kind = kind;
            DraftIds = draftIds ?? Array.Empty<Guid>();
            TotalWorkingCount = totalWorking;
            Key = key;
        }
    }
}
