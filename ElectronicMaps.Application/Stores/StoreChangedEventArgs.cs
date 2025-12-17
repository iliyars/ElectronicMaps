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

        /// <summary>
        /// Текущее количество элементов в Store после изменения.
        /// </summary>
        public int Count { get; }

        public StoreChangedEventArgs(StoreChangeKind kind, int totalWorkingCount, IReadOnlyList<Guid>? draftIds, string? key)
        {
            Kind = kind;
            TotalWorkingCount = totalWorkingCount;
            DraftIds = draftIds ?? Array.Empty<Guid>();
            Key = key;
        }

        public static StoreChangedEventArgs ForSingleDraft(
           StoreChangeKind kind,
           int totalWorkingCount,
           Guid draftId,
           string? Key = null)
           => new(kind, totalWorkingCount, new[] { draftId }, Key);

        public static StoreChangedEventArgs ForDrafts(
           StoreChangeKind kind,
           int totalWorkingCount,
           IReadOnlyList<Guid> draftIds,
           string? Key = null)
           => new(kind, totalWorkingCount, draftIds, Key);
    }
}
