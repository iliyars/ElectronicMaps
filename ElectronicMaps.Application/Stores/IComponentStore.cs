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


        #region Import

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
        #endregion

        #region Views
        // --- Views (сохранённые сортировки) ---
        void RebuildViewsByForms();
        IReadOnlyList<string> GetViewKeys();
        IReadOnlyList<ComponentDraft> GetWorkingForView(string key);
        void SaveView(string key, IEnumerable<Guid> draftsId);
        bool RemoveView(string key);
        #endregion

        #region working
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
        ComponentDraft? TryGetWorking(Guid draftId);
        bool UpdateWorking(Guid draftId, Func<ComponentDraft, ComponentDraft> update);
        void RemoveWorking(IEnumerable<Guid> Ids);
        void ReplaceWorking(IEnumerable<ComponentDraft> components);
        IReadOnlyList<ComponentDraft> SplitWorking(Guid draftId, int parts);
        ComponentDraft MergeWorking(IReadOnlyList<Guid> draftIds);
        public void AssignFamily(Guid componentDraftId, string? familyName, int? dbFamilyId);
        #endregion

        #region Documents
        // --- Documents metadata ---
        IReadOnlyList<WordDocumentInfo> GetDocuments();
        void AddDocument(WordDocumentInfo doc);
        bool RemoveDocument(Guid documentId);

        bool RemoveDocumentBinary(Guid documentId);

        byte[]? GetDocumentBinary(Guid documentId);
        #endregion

        #region Project IO
        // --- Project I/O ---
        Task SaveProjectAsync(string filePath, CancellationToken ct = default);
        Task LoadProjectAsync(string filePath, CancellationToken ct = default);
#endregion

        void Clear();
        void MarkClean();



    }
}
