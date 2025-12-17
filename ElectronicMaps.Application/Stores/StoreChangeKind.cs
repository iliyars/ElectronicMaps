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

        ImportReplaced,
        WorkingInitialized,
        WorkingReplaced,

        WorkingUpserted,     // добавили или обновили один draft
        WorkingRemoved,      // удалили один или несколько

        ViewsRebuilt,
        ViewSaved,
        ViewRemoved,

        DocumentsChanged,
        ProjectLoaded,
        ProjectSaved,

        Cleared,
        MarkedClean
    }
}
