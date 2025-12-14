using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.WorkspaceProject.Models;

// Сущности для работы и хранения данных в рабочем проекте:

// ImportedRows - результат импорт XML
// не редактируется пользователем, не хранит параметры компонента (параметры в рабочем слое)
// Используется для таблицы в которой отображаются все прочитанные строки
// Пример: "Микросхема 5584АП7У, кол. 1 шт, Обозначение DD1, Семейство 5584АП7У, в БД найден/не найдено, форма семейсатва= FORM_4, форма компонента FORM_64"

// WorkingComponents - рабочий набор компонентов проекта, то с чем пользователь работает (редактиркет параметры, делит/объединяет по дезигаторам) из них формируется Word-документ
// !!! Может быть несколько рабочх компонентов на один ImportedRow

// ComponentDraft - рабочая сущность компонента в контексте конкретной формы. То есть компонент подготовленный под конкретную форму (с параметрами).
// У одного импортируемого компонента (ImportRow) есть две формы:
// Форма 4 (семейство) и форма по типу компонента
// Пример: Прочитали из xml микросхему 5584АП7У - создали два Draft'a под форму 4 и под форму 64

//ViewsByKey - сохранённые "сортировки" / представлеиня
// Dictionary<string, List<Guid>> хранит ID рабочих компонентов (ComponentDraft.ID)
// Пример 

public record WorkspaceProject(

        // Результат импорта XML
        IReadOnlyList<ImportedRow> ImportedRows,

        // Сохранённые "сортировки" / представлеиня
        IReadOnlyDictionary<string, List<Guid>> ViewsByKey,

        // Рабочие компоненты 
        IReadOnlyDictionary<Guid, ComponentDraft> WorkingComponents,

        // Созданные Word-документы (метаданные)
        IReadOnlyList<WordDocumentInfo> Documents
        )
{
    public static WorkspaceProject Empty() =>
        new(
            ImportedRows: Array.Empty<ImportedRow>(),
            ViewsByKey: new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase),
            WorkingComponents: new Dictionary<Guid, ComponentDraft>(),
            Documents: Array.Empty<WordDocumentInfo>()
            );
}
