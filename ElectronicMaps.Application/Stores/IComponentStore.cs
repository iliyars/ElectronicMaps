using ElectronicMaps.Application.WorkspaceProject.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    /// <summary>
    /// Хранилище анализированных компонентов в памяти.
    /// Компоненты группируются по строковому ключу (например, коду формы).
    /// Все компоненты могуть храняиться по ключу "all"
    /// 
    /// Хранилище потокобезопасно и генерирует событие <see cref="Changed"/> при любых изменениях.
    /// </summary>
    public interface IComponentStore : IDisposable
    {
        /// <summary>
        /// Собитие, возникающее при любых изменениях в хранилище.
        /// Вызывается после операций добавления, удаления, замены, загрузки и сохранения.
        /// В аргументах события передаётся тип изменения, ключ (если применимо)
        /// и текущее общее количество компонентов во всех группах.
        /// </summary>
        event EventHandler<StoreChangedEventArgs>? Changed;
        bool HasUnsavedChanges { get; }

        WorkspaceProject.Models.WorkspaceProject Current { get; }

        /// <summary>
        /// Возвращает все компоненты из хранилища, независимо от группировки.
        /// 
        /// Реализация возвращает копию списка. Изменения возвращаемого списка не влияют на хранилище.
        /// </summary>
        /// <returns>Список компонентов. Может быть пустым но никогда не null</returns>
        IReadOnlyList<ImportedRow> GetAllImported();
        /// <summary>
        /// Полностью заменяет содержимое хранилища новым набором компонентов.
        /// 
        /// Типичный сценарий использования — первичная загрузка перечня компонентов
        /// после анализа нового XML-файла. Реализация может сохранить этот набор,
        /// например, под специальным ключом (условно "ALL"), но детали ключа
        /// являются внутренней реализацией.
        /// </summary>
        /// <param name="components">
        /// Последовательность компонентов, которая должна стать единственным
        /// содержимым хранилища. Значения с пустым <see cref="ImportedRow.CleanName"/>
        /// могут игнорироваться реализацией.
        /// </param>
        void ReplaceImport(IEnumerable<ImportedRow> components);

        // --- Views (сохранённые сортировки) ---
        void RebuildViewsByForms();
        IReadOnlyList<string> GetViewKeys();
        IReadOnlyList<ComponentDraft> GetWorkingForView(string key);
        void SaveView(string key, IEnumerable<Guid> importedRowIds);
        bool RemoveView(string key);

        // --- Working components (editable) ---
        /// <summary>
        /// Инициализирует рабочие компоненты (<see cref="ComponentDraft"/>)
        /// на основе импортированных строк (<see cref="ImportedRow"/>),
        /// создавая по одному Draft'у для каждой поддерживаемой формы
        /// например, форма 4 и форма 64).
        /// 
        /// Метод используется при первичной инициализации Workspace
        /// после импорта XML и не выполняет дробление компонентов.
        /// </summary>
        void InitializeWorkingDrafts();
        IReadOnlyList<ComponentDraft> GetAllWorking();
        void ReplaceWorking(IEnumerable<ComponentDraft> components);
        void UpdateWorking(Guid draftId, Func<ComponentDraft, ComponentDraft> update);
        ComponentDraft? TryGetWorking(Guid draftId);
        bool RemoveWorking(Guid id);
        IReadOnlyList<ComponentDraft> SplitWorking(Guid draftId, int parts);

        ComponentDraft MergeWorking(IReadOnlyList<Guid> draftIds);

        // --- Documents metadata ---
        IReadOnlyList<WordDocumentInfo> GetDocuments();
        void AddDocument(WordDocumentInfo doc);
        bool RemoveDocument(Guid documentId);

        public bool RemoveDocumentBinary(Guid documentId);

        public byte[]? GetDocumentBinary(Guid documentId);

        // --- Project I/O ---
        Task SaveProjectAsync(string filePath, CancellationToken ct = default);
        Task LoadProjectAsync(string filePath, CancellationToken ct = default);

        void Clear();
        void MarkClean();



    }
}
