using ElectronicMaps.Application.Abstractons.Queries;
using ElectronicMaps.Application.DTO.Parameters;
using ElectronicMaps.Application.Utils;
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
                try { return _current; }
                finally { _lock.ExitReadLock(); }
            }
        }

        public ComponentStore(IWorkspaceProjectSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _current = WorkspaceProject.Models.WorkspaceProject.Empty();
            _hasUnsavedChanges = false;
        }

        #region Import
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

        #endregion

        #region Views
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

        public void SaveView(string key, IEnumerable<Guid> draftIds)
        {
            if(string.IsNullOrWhiteSpace(key))
                throw new ArgumentException("View key must not be empty.", nameof(key));
            if(draftIds == null)
                throw new ArgumentNullException(nameof(draftIds));

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                var existingDraftIds = _current.WorkingComponents.Keys.ToHashSet();

                var ids = draftIds
                    .Where(id => id != Guid.Empty && existingDraftIds.Contains(id))
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
        /// <summary>
        /// Перестраивает представления (<see cref="WorkspaceProject.ViewsByKey"/>) по формам
        /// на основе текущих рабочих компонентов (<see cref="WorkspaceProject.WorkingComponents"/>).
        ///
        /// Для каждой формы создаётся view с ключом вида "FORM_CODE", содержащий список идентификаторов
        /// рабочих компонентов (<see cref="ComponentDraft.Id"/>), относящихся к этой форме.
        /// </summary>
        public void RebuildViewsByForms()
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                var working = _current.WorkingComponents;
                var views = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase);

                if (working is not null && working.Count > 0)
                {
                    var grouped = working.Values
                        .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.FormCode))
                        .GroupBy(d => d.FormCode, StringComparer.OrdinalIgnoreCase);

                    foreach (var g in grouped)
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
            finally
            {
                _lock.ExitWriteLock();
            }
            Changed?.Invoke(this, args);
        }
        #endregion

        #region Working
        // ---------------- Working components ----------------
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
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();

            try
            {
                var imported = _current.ImportedRows;
                var working = new Dictionary<Guid, ComponentDraft>();

                //Группируем по семействам
                var familyGroups = imported
                    .Where(r => r is not null)
                    .GroupBy(GetFamilyGroupKey, StringComparer.OrdinalIgnoreCase);

                foreach(var g in familyGroups)
                {
                    var rows = g.ToList();
                    if (rows.Count == 0) continue;

                    var draft = CreateFamilyDraftFromGroup(rows);
                    working[draft.Id] = draft;
                }

                if(imported is not null && imported.Count > 0)
                {
                    foreach(var row in imported)
                    {
                      

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

        private ComponentDraft CreateFamilyDraftFromGroup(List<ImportedRow> rows)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));
            if (rows.Count == 0) throw new ArgumentException("Group is empty.", nameof(rows));

            var main = rows[0];

            var formCode = main.FamilyFormTypeCode!;
            var formName = main.FamilyFormDisplayName ?? formCode;

            
            var mergeDesignators = rows
                .SelectMany(r => r.Designators ?? Array.Empty<string>())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var quantitySum = rows.Sum(r => r.Quantity);
            var quantity = mergeDesignators.Count > 0 ? mergeDesignators.Count : quantitySum;

            var familyName = main.Family;
            var name = !string.IsNullOrWhiteSpace(familyName) ? familyName! : main.CleanName;

            var emptyParams = new Dictionary<int, ParameterValueDraft>();

            return new ComponentDraft(
                Id: Guid.NewGuid(),
                SourceRowId: main.RowId,

                Name: name,
                DBComponentId: null,
                Family:familyName,
                DbFamilyId: main.DatabaseFamilyId,

                FormCode: formCode,
                FormName: formName,

                Quantity: quantity,
                Designators: mergeDesignators,

                SelectedRemarksIds: Array.Empty<int>(),

                NdtParametersOverrides: emptyParams,
                SchematicParameters: emptyParams
                );
        }

        public IReadOnlyList<ComponentDraft> GetAllWorking()
        {
            _lock.EnterReadLock();
            try { return _current.WorkingComponents.Values.ToList(); }
            finally { _lock.ExitReadLock(); }
        }

        //public void UpsertWorking(ComponentDraft component)
        //{
        //    if (component.Id == Guid.Empty)
        //        throw new ArgumentException("ComponentDraft.Id is empty.", nameof(component));

        //    StoreChangedEventArgs? args;
        //    _lock.EnterWriteLock();
        //    try
        //    {
        //        var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
        //        dict[component.Id] = component;
        //        _current = _current with { WorkingComponents = dict };

        //        MarkDirty_NoLock();

        //        args = BuildArgs_NoLock(StoreChangeKind.WorkingUpserted, key: "WORKING", draftIds: new[] { component.Id });

        //    }
        //    finally { _lock.ExitWriteLock(); }

        //    Changed?.Invoke(this, args);
        //}

        public bool UpdateWorking(Guid draftId, Func<ComponentDraft, ComponentDraft> update)
        {
            StoreChangedEventArgs? args = null;
            var updatedOk = false;
            _lock.EnterWriteLock();

            try
            {
                if (!_current.WorkingComponents.TryGetValue(draftId, out var current))
                    return false; // throw new KeyNotFoundException(...)

                var updated = update(current);

                if (updated.Id != draftId)
                    throw new InvalidOperationException("UpdateWorking: нельзя менять ComponentDraft.Id.");

                if (Equals(updated, current))
                    return true;


                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents)
                {
                    [draftId] = updated
                };

                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingUpdated, key: "WORKING", draftIds: new[] { draftId });

                updatedOk = true;
            }
            finally { _lock.ExitWriteLock(); }

            if(args !=null)
            {
                Changed?.Invoke(this, args);
            }
            return updatedOk;
        }

        public ComponentDraft? TryGetWorking(Guid draftId)
        {
            _lock.EnterReadLock();
            try
            {
                return _current.WorkingComponents.TryGetValue(draftId, out var d) ? d : null;
            }
            finally { _lock.ExitReadLock(); }
        }
        //TODO: переделать
        public IReadOnlyList<ComponentDraft> SplitWorking(Guid draftId, int parts)
        {
            StoreChangedEventArgs? removedArgs = null;
            StoreChangedEventArgs? upsertArgs = null;
            List<ComponentDraft> created = new();

            _lock.EnterWriteLock();
            try
            {
                if (!_current.WorkingComponents.TryGetValue(draftId, out var source))
                    return Array.Empty<ComponentDraft>();

                // БАЗОВЫЙ split: делим quantity примерно поровну, designators режем по группам
                var qty = source.Quantity;
                var baseQty = qty / parts;
                var rest = qty % parts;

                var des = source.Designators?.ToList() ?? new List<string>();
                var per = des.Count == 0 ? 0 : (int)Math.Ceiling(des.Count / (double)parts);

                for (int i = 0; i < parts; i++)
                {
                    var q = baseQty + (i < rest ? 1 : 0);
                    if (q <= 0) q = 1;

                    var slice = (per == 0) ? Array.Empty<string>()
                        : des.Skip(i * per).Take(per).ToArray();

                    var d = source with
                    {
                        Id = Guid.NewGuid(),
                        Quantity = q,
                        Designators = slice
                    };

                    created.Add(d);
                }

                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                dict.Remove(draftId);
                foreach (var d in created)
                    dict[d.Id] = d;

                _current = _current with { WorkingComponents = dict };
                MarkDirty_NoLock();

                removedArgs = BuildArgs_NoLock(StoreChangeKind.WorkingRemoved, key: "WORKING", draftIds: new[] { draftId });
                upsertArgs = BuildArgs_NoLock(StoreChangeKind.WorkingUpserted, key: "WORKING", draftIds: created.Select(x => x.Id).ToList());
            }
            finally { _lock.ExitWriteLock(); }

            if (removedArgs != null) Changed?.Invoke(this, removedArgs);
            if (upsertArgs != null) Changed?.Invoke(this, upsertArgs);

            return created;
        }

        public ComponentDraft MergeWorking(IReadOnlyList<Guid> draftIds)
        {
            StoreChangedEventArgs? removedArgs = null;
            StoreChangedEventArgs? upsertArgs = null;

            ComponentDraft merged;

            _lock.EnterWriteLock();
            try
            {
                var dictOld = _current.WorkingComponents;

                var parts = new List<ComponentDraft>(draftIds.Count);
                foreach (var id in draftIds)
                    if (dictOld.TryGetValue(id, out var d)) parts.Add(d);

                if(parts.Count < 2)
                {
                    throw new InvalidOperationException("MergeWorking: не найдены все указанные drafts.");
                }

                // Простейшие правила merge: берем “первый” как основу
                var first = parts[0];

                merged = first with
                {
                    Id = Guid.NewGuid(),
                    Quantity = parts.Sum(p => p.Quantity),
                    Designators = parts.SelectMany(p => p.Designators ?? Array.Empty<string>()).Distinct().ToArray(), //TODO: Сделатьб static метод для работы с designtor'ами
                };

                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                foreach (var id in draftIds) dict.Remove(id);
                dict[merged.Id] = merged;

                _current = _current with { WorkingComponents = dict };
                MarkDirty_NoLock();

                removedArgs = BuildArgs_NoLock(StoreChangeKind.WorkingRemoved, key: "WORKING", draftIds: draftIds);
                upsertArgs = BuildArgs_NoLock(StoreChangeKind.WorkingUpserted, key: "WORKING", draftIds: new[] { merged.Id });
            }
            finally { _lock.ExitWriteLock(); }

            if (removedArgs != null) Changed?.Invoke(this, removedArgs);
            if(upsertArgs != null) Changed?.Invoke(this, upsertArgs);

            return merged;
        }

        public void RemoveWorking(IEnumerable<Guid> ids)
        {
            StoreChangedEventArgs? args = null;
            var removed = false;

            _lock.EnterWriteLock();
            try
            {
                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                var removedAny = false;

                foreach(var id in ids)
                {
                    removedAny |= dict.Remove(id);
                }

                if (!removedAny) return;

                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();

                args = BuildArgs_NoLock(StoreChangeKind.WorkingRemoved, key: "WORKING", draftIds:  ids.ToList());
            }
            finally { _lock.ExitWriteLock(); }

            if(args is not null)
                Changed?.Invoke(this, args);
        }

        public void ReplaceWorking(IEnumerable<ComponentDraft> components)
        {
            var dict = components
                .Where(c => c is not null && c.Id != Guid.Empty)
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
        #endregion


        #region Documents
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

        #endregion


        #region IO
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


        #endregion
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


        
        private ComponentDraft CreateDraftFromImportedRow(ImportedRow row, string formCode)
        {
            var emptyParams = new Dictionary<int, ParameterValueDraft>();
            var designators = DesignatorHelper.Expand(row.Designator);

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

        private StoreChangedEventArgs BuildArgs_NoLock(StoreChangeKind kind, string? key, IReadOnlyList<Guid>? draftIds)
        {
            var totalWorking = _current.WorkingComponents?.Count ?? 0;
            return new StoreChangedEventArgs(kind, key, totalWorking, draftIds);
        }

        private static string GetFamilyGroupKey(ImportedRow r)
        {
            if (r.DatabaseFamilyId is int id && id > 0)
                return $"DBF:{id}";

            if(!string.IsNullOrWhiteSpace(r.Family))
                return  $"FAM:{NormalizeKey(r.Family)}";

            return $"NOFAM: {NormalizeKey(r.Type)} | {NormalizeKey(r.CleanName)}";
        }

        private static string NormalizeKey(string s)
        {
            s = s.Trim().ToUpperInvariant();
            return string.Join(
                ' ',
                s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

    }
}

