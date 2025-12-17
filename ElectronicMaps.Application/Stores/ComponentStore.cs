using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Application.WorkspaceProject;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Runtime;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Stores
{
    public sealed class ComponentStore : IComponentStore
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
        private readonly IWorkspaceProjectSerializer _serializer;

        private WorkspaceProject.Models.WorkspaceProject _current;
        private Dictionary<Guid, byte[]> _documentBinaries = new();
        private bool _hasUnsavedChanges;

        public event EventHandler<StoreChangedEventArgs>? Changed;

        public bool HasUnsavedChanges => _hasUnsavedChanges;

        public WorkspaceProject.Models.WorkspaceProject Current
        {
            get
            {
                _lock.EnterReadLock();
                try
                {
                    return _current;
                }
                finally
                {
                    _lock.ExitReadLock();
                }
            }
        }

        public ComponentStore(IWorkspaceProjectSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _current = WorkspaceProject.Models.WorkspaceProject.Empty();
            _hasUnsavedChanges = false;
        }


        // ---------------- Import ----------------
        public IReadOnlyList<ImportedRow> GetAllImported()
        {
            _lock.EnterReadLock();
            try { return _current.ImportedRows; }
            finally { _lock.ExitReadLock(); }
        }

        public void ReplaceImport(IEnumerable<ImportedRow> importedRows)
        {
            if(importedRows is null) throw new ArgumentNullException(nameof(importedRows));

            var list = importedRows
                .Where(r => r is not null && r.RowId != Guid.Empty && !string.IsNullOrWhiteSpace(r.CleanName))
                .ToList();

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _current = _current with
                {
                    ImportedRows = list,
                    ViewsByKey = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase),
                    WorkingComponents = new Dictionary<Guid, ComponentDraft>() // рабочий слой тоже обычно пересобирается
                };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.ImportReplaced, key: "ALL", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }



        // ---- Views --- // 
        public IReadOnlyList<string> GetViewKeys()
        {
            _lock.EnterReadLock();
            try { return _current.ViewsByKey.Keys.OrderBy(x => x).ToList(); }
            finally { _lock.ExitReadLock(); }
        }

        /// <summary>
        /// возвращает рабочие компоненты (<see cref="ComponentDraft"/>) для указанного <paramref name="key"/>
        /// 
        /// View представляет собой список идентификаторов рабочих компонентов
        /// (<see cref="ComponentDraft.Id"/>), сгруппированных, например,
        /// по форме (форма 4, форма 64 и т.п.).
        /// 
        /// Метод не работает с результатами импорта (<see cref="ImportedRow"/>),
        /// (<see cref="WorkspaceProject.WorkingComponents"/>).
        /// 
        /// <param name="key">
        /// Ключ представления (например, "FORM_4", "FORM64").
        /// </param>
        /// </summary>
        /// 
        /// <returns>
        /// Список рабочих компонентов, принадлежащих данному представлению.
        /// Если представление не существует или пусто, возвращается пустой список.
        /// </returns>
        public IReadOnlyList<ComponentDraft> GetWorkingForView(string key)
        {
            if(string.IsNullOrWhiteSpace(key))
                return Array.Empty<ComponentDraft>();

            _lock.EnterReadLock();
            try
            {
                if (!_current.ViewsByKey.TryGetValue(key, out var ids) || ids.Count == 0)
                    return Array.Empty<ComponentDraft>();

                var dict = _current.WorkingComponents;
                if (dict.Count == 0) return Array.Empty<ComponentDraft>();

                var result = new List<ComponentDraft>(ids.Count);
                foreach(var id in ids)
                {
                    if(dict.TryGetValue(id, out var comp))
                        result.Add(comp);
                }
                return result;
            }
            finally { _lock.ExitReadLock(); }
        }

        public void SaveView(string key, IEnumerable<Guid> importedRowIds)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("View key must not be empty.", nameof(key));
            if(importedRowIds == null)
                throw new ArgumentNullException(nameof(importedRowIds));

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                var existingIds = _current.ImportedRows.Select(x => x.RowId).ToHashSet();

                var ids = importedRowIds
                    .Where(id => id != Guid.Empty && existingIds.Contains(id))
                    .Distinct()
                    .ToList();

                var newViews = new Dictionary<string, List<Guid>>(_current.ViewsByKey, StringComparer.OrdinalIgnoreCase)
                {
                    [key] = ids
                };

                _current = _current with { ViewsByKey = newViews };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.ViewSaved, key:key, draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public bool RemoveView(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            StoreChangedEventArgs? args = null;
            var removed = false;

            _lock.EnterWriteLock();
            try
            {
                if (!_current.ViewsByKey.ContainsKey(key))
                    return false;

                var newViews = new Dictionary<string, List<Guid>>(_current.ViewsByKey, StringComparer.OrdinalIgnoreCase);
                removed = newViews.Remove(key);
                if (!removed) return false;

                _current = _current with { ViewsByKey = newViews };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.ViewRemoved, key: key, draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
            return removed;

        }

        public void RebuidViewsByForms()
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();

            try
            {
                var working = _current.WorkingComponents;
                var views = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase);

                if(working is not null && working.Count > 0)
                {
                    var grouped = working.Values
                        .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.FormCode))
                        .GroupBy(d => d.FormCode, StringComparer.OrdinalIgnoreCase);

                    foreach(var g in grouped)
                    {
                        var ids = g.OrderBy(d => d.SourceRowId)
                                    .ThenBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                                    .Select(d => d.Id)
                                    .ToList();

                        views[g.Key] = ids;
                    }
                }

                _current = _current with { ViewsByKey = views };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.ViewsRebuilt, key: "VIEWS", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        // ---------------- Working components ----------------
        public void InitializeWorkingDrafts()
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();

            try
            {
                var imported = _current.ImportedRows;
                var working = new Dictionary<Guid, ComponentDraft>();

                if(imported is not null && imported.Count > 0)
                {
                    foreach(var row in imported)
                    {
                        //TODO: Перед созданием необходимо сложить все компоненты одного семейства
                        //Draft формы семейства
                        var familyFormCode = row.FamilyFormTypeCode;
                        var draft = CreateDraftFromImportedRow(row, familyFormCode);
                        working[draft.Id] = draft;

                        //Draft типа компонента
                        var componentFormCode = row.ComponentFormCode;
                        var draf = CreateDraftFromImportedRow(row, componentFormCode);
                        working[draf.Id] = draf;
                    }
                }

                _current = _current with { WorkingComponents = working };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingInitialized, key: "WORKING", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public IReadOnlyList<ComponentDraft> GetAllWorking()
        {
            _lock.EnterReadLock();
            try { return _current.WorkingComponents.Values.ToList(); }
            finally { _lock.ExitReadLock(); }
        }

        public void ReplaceWorking(IEnumerable<ComponentDraft> components)
        {
            if (components is null) throw new ArgumentNullException(nameof(components));

            var dict = components
                .Where(c => c.Id != Guid.Empty)
                .ToDictionary(c => c.Id, c => c);

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _current = _current with { WorkingComponents = dict };
                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingReplaced, key: "WORKING", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public void UpsertWorking(ComponentDraft component)
        {
            if (component.Id == Guid.Empty)
                throw new ArgumentException("ComponentDraft.Id is empty.", nameof(component));

            StoreChangedEventArgs? args;
            _lock.EnterWriteLock();
            try
            {
                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                dict[component.Id] = component;
                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingUpserted, key: "WORKING", draftIds: new[] { component.Id });

            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public bool RemoveWorking(Guid id)
        {
            if (id == Guid.Empty) return false;

            StoreChangedEventArgs? args = null;
            var removed = false;

            _lock.EnterWriteLock();
            try
            {
                if (!_current.WorkingComponents.ContainsKey(id)) return false;

                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                removed = dict.Remove(id);
                if (!removed) return false;

                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingRemoved, key: "WORKING", draftIds: new[] { id });
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);

            return removed;
        }

        // ---------------- Documents ----------------

        public IReadOnlyList<WordDocumentInfo> GetDocuments()
        {
            _lock.EnterReadLock();
            try { return _current.Documents; }
            finally { _lock.ExitReadLock(); }
        }

        public void AddDocument(WordDocumentInfo doc)
        {
            if (doc.DocumentId == Guid.Empty)
                throw new ArgumentException("DocumentId is empty.", nameof(doc));

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                var list = _current.Documents.ToList();
                list.Add(doc);

                _current = _current with { Documents = list };

                MarkDirty_NoLock();
                args = BuildArgs_NoLock(StoreChangeKind.DocumentsChanged, key: "DOCS", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public bool RemoveDocument(Guid documentId)
        {
            if (documentId == Guid.Empty)
                return false;

            StoreChangedEventArgs? args = null;

            var removed = false;

            _lock.EnterWriteLock();
            try
            {
                var list = _current.Documents.ToList();
                removed = list.RemoveAll(d => d.DocumentId == documentId) > 0;
                if (!removed) return false;

                _current = _current with { Documents = list };

                _documentBinaries.Remove(documentId);
                MarkDirty_NoLock();
                args = BuildArgs_NoLock(StoreChangeKind.DocumentsChanged, key: "DOCS", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
            return removed;
        }

        public void AddOrReplaceDocumentBinary(Guid documentId, byte[] content)
        {
            if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is empty.", nameof(documentId));
            if (content is null || content.Length == 0) throw new ArgumentException("Content is empty.", nameof(content));

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _documentBinaries[documentId] = content;
                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.DocumentsChanged, key: "DOCS_BIN", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public bool RemoveDocumentBinary(Guid documentId)
        {
            if (documentId == Guid.Empty) return false;

            StoreChangedEventArgs? args = null;
            var removed = false;

            _lock.EnterWriteLock();
            try
            {
                removed = _documentBinaries.Remove(documentId);
                if (!removed) return false;

                MarkDirty_NoLock();
                args = BuildArgs_NoLock(StoreChangeKind.DocumentsChanged, key: "DOCS_BIN", draftIds: null);

            }
            finally { _lock.ExitWriteLock(); }
            Changed?.Invoke(this, args);
            return removed;
        }


        public byte[]? GetDocumentBinary(Guid documentId)
        {
            _lock.EnterReadLock();
            try
            {
                return _documentBinaries.TryGetValue(documentId, out var bytes) ? bytes : null;
            }
            finally { _lock.ExitReadLock(); }
        }

        // ---------------- Project I/O ----------------

        public async Task SaveProjectAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            WorkspaceProject.Models.WorkspaceProject snapshot;
            IReadOnlyCollection<WordDocumentBinary> docs;

            _lock.EnterReadLock();
            try 
            {
                snapshot = _current;
                docs = _documentBinaries.Select(kv => new WordDocumentBinary(kv.Key, kv.Value)).ToList();
            }
            finally { _lock.ExitReadLock(); }
            // docsBytes берём из отдельного хранилища документов
            await _serializer.SaveAsync(filePath, snapshot, docs, ct).ConfigureAwait(false);

            StoreChangedEventArgs args;
            _lock.EnterWriteLock();
            try
            {
                _hasUnsavedChanges = false;
                args = BuildArgs_NoLock(StoreChangeKind.ProjectSaved, key: filePath, draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public async Task LoadProjectAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            var loaded = await _serializer.LoadAsync(filePath, ct);

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _current = loaded.Project;

                _documentBinaries = loaded.Documents.GroupBy(d => d.DocumentId).ToDictionary(g => g.Key, g => g.Last().Content);
                _hasUnsavedChanges = false;
                args = BuildArgs_NoLock(StoreChangeKind.ProjectLoaded, key: filePath, draftIds: null);

            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public void Clear()
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _current = WorkspaceProject.Models.WorkspaceProject.Empty();
                _documentBinaries.Clear();
                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.Cleared, key: null, draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public void MarkClean() 
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _hasUnsavedChanges = false;
                args = BuildArgs_NoLock(StoreChangeKind.MarkedClean, key: null, draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }

        public void Dispose() => _lock.Dispose();

        private void MarkDirty_NoLock() => _hasUnsavedChanges = true;

        private void OnChanged(StoreChangeKind kind, string? key)
            => Changed?.Invoke(this, new StoreChangedEventArgs(kind, key, _current.ImportedRows.Count));
        /// <summary>
        /// Перестраивает представления (<see cref="WorkspaceProject.ViewsByKey"/>) по формам
        /// на основе текущих рабочих компонентов (<see cref="WorkspaceProject.WorkingComponents"/>).
        ///
        /// Для каждой формы создаётся view с ключом вида "FORM_CODE", содержащий список идентификаторов
        /// рабочих компонентов (<see cref="ComponentDraft.Id"/>), относящихся к этой форме.
        /// </summary>
        public void RebuildViewsByForms()
        {
            _lock.EnterWriteLock();
            try
            {
                var working = _current.WorkingComponents;
                if(working is null || working.Count == 0)
                {
                    _current = _current with
                    {
                        ViewsByKey = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase)
                    };

                    MarkDirty_NoLock();
                    OnChanged(StoreChangeKind.Replaced, "VIEWS");
                    return;
                }

                // Группировка рабочие компоненты по коду формы
                var grouped = working.Values
                    .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.FormCode))
                    .GroupBy(d => d.FormCode, StringComparer.OrdinalIgnoreCase);

                var views = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase);

                foreach(var g in grouped)
                {
                    var formCode = g.Key;
                    var key = formCode;

                    var ids = g.OrderBy(d => d.SourceRowId)
                                .ThenBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                                .Select(d => d.Id)
                                .ToList();

                    views[key] = ids;
                }

                _current = _current with
                {
                    ViewsByKey = views
                };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Replaced, "VIEWS");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        /// <summary>
        /// Инициализирует рабочие компоненты (<see cref="ComponentDraft"/>) на основе импортированных строк
        /// (<see cref="ImportedRow"/>), создавая по одному Draft'у на каждую применимую форму:
        /// форму семейства (например "4") и форму типа компонента (например "64").
        /// 
        /// Метод пересобирает <see cref="WorkspaceProject.WorkingComponents"/> "с нуля".
        /// Параметры не подгружаются здесь и инициализируются пустыми — их загрузка выполняется отдельным шагом.
        /// </summary>
        public void InitializeWorkingDrafts()
        {
            _lock.EnterWriteLock();
            try
            {
                var imported = _current.ImportedRows;
                if(imported is null || imported.Count == 0)
                {
                    _current = _current with { WorkingComponents = new Dictionary<Guid, ComponentDraft>() };
                    MarkDirty_NoLock();
                    OnChanged(StoreChangeKind.Replaced, "WORKING");
                    return;
                }

                var working = new Dictionary<Guid, ComponentDraft>();

                foreach(var row in imported)
                {
                    if (row is null) continue;
                    if (row.RowId == Guid.Empty) continue;

                    // Драфт формы семейства
                    var familyFormCode = row.FamilyFormTypeCode;
                    if(!string.IsNullOrEmpty(familyFormCode))
                    {
                        var draft = CreateDraftFromImportedRow(row, familyFormCode);
                        working[draft.Id] = draft;
                    }

                    //Драфт формы типа компонента
                    var componentFormCode = row.ComponentFormCode;
                    if(!string.IsNullOrWhiteSpace(componentFormCode))
                    {
                        var draft = CreateDraftFromImportedRow(row, componentFormCode);
                        working[draft.Id] = draft;
                    }
                }

                _current = _current with { WorkingComponents = working };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Replaced, "WORKING");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        private ComponentDraft CreateDraftFromImportedRow(ImportedRow row, string formCode)
        {
            var emptyParams = new Dictionary<int, ParameterValueDraft>();
            var designators = row.Designators ?? Array.Empty<string>();

            return new ComponentDraft(
                Id: Guid.NewGuid(),
                SourceRowId: row.RowId,
                Name: row.CleanName,
                DBComponentId: row.ExistingComponentId,
                DbFamilyId: row.DatabaseFamilyId,
                Family: row.Family,
                FormCode: formCode,
                FormName: row.ComponentFormDisplayName,
                Quantity: row.Quantity,
                SelectedRemarksIds: new List<int>(),
                Designators: designators,
                NdtParametersOverrides: emptyParams,
                SchematicParameters: emptyParams);
        }

        private void RaiseChanged_NoLock(StoreChangeKind kind, string? key, IReadOnlyList<Guid>? draftIds)
        {
            var totalWorking = _current.WorkingComponents?.Count ?? 0;
            var args = new StoreChangedEventArgs(kind, totalWorking, draftIds, key);
            Changed?.Invoke(this, args);
        }

        private StoreChangedEventArgs BuildArgs_NoLock(StoreChangeKind importReplaced, string key, object draftIds)
        {
            throw new NotImplementedException();
        }
    }
}

