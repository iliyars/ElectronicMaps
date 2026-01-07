using ElectronicMaps.Application.Common.Helpers;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Application.Features.Workspace.Serialization;
using System.Diagnostics;


namespace ElectronicMaps.Application.Stores
{
    public sealed class ComponentStore : IComponentStore
    {
        private readonly ReaderWriterLockSlim _lock = new(LockRecursionPolicy.NoRecursion);
        private readonly IWorkspaceProjectSerializer _serializer;

        private Features.Workspace.Models.WorkspaceProject _current;
        private Dictionary<Guid, byte[]> _documentBinaries = new();
        private bool _hasUnsavedChanges;


        // Индексы и КЕШИ
        /// <summary>
        /// Индекс: FormCode → List[ComponentDraft]
        /// Кеш для GetWorkingForView() - O(1) вместо O(N)
        /// </summary>
        private Dictionary<string, List<ComponentDraft>> _draftsByFormCode = new(StringComparer.OrdinalIgnoreCase);

        /// <summary>
        /// Индекс: DraftId → ComponentDraft
        /// Кеш для TryGetWorking() - O(1) вместо O(N)
        /// </summary>
        private Dictionary<Guid, ComponentDraft> _draftsById = new();

        /// <summary>
        /// Кеш отсортированных ключей форм для GetViewKeys()
        /// </summary>
        private List<string> _sortedFormCodes = new();

        /// <summary>
        /// Флаг валидности индексов
        /// </summary>
        private bool _indexesValid = false;

        private static readonly ApprovalRef MissingApproval =
            new(ApprovalStatus.Missing, PendingSetId: null, ApprovedSetId: null);


        public event EventHandler<StoreChangedEventArgs>? Changed;

        public bool HasUnsavedChanges => _hasUnsavedChanges;

        public Features.Workspace.Models.WorkspaceProject Current
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
            _current = Features.Workspace.Models.WorkspaceProject.Empty();
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

                InvalidateIndexes_NoLock();

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
            try 
            {
                EnsureIndexes_NoLock();
                return _sortedFormCodes;//_current.ViewsByKey.Keys.OrderBy(x => x).ToList(); 
            }
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
                EnsureIndexes_NoLock();

                // O(1) lookup в индексе!
                return _draftsByFormCode.TryGetValue(key, out var list)
                    ? list
                    : Array.Empty<ComponentDraft>();
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
                RebuildViewsByForms_NoLock();

                // Перестраиваем индексы после изменения Views
                RebuildIndexes_NoLock();

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
        public void InitializeWorking(IReadOnlyList<ImportedRow> analyzedRows)
        {
            if (analyzedRows == null)
                throw new ArgumentNullException(nameof(analyzedRows));

            StoreChangedEventArgs args;

            _lock.EnterWriteLock();

            try
            {
                var sw = Stopwatch.StartNew();

                var working = new Dictionary<Guid, ComponentDraft>();

                if(analyzedRows.Count > 0)
                {
                    var familyGroups = analyzedRows
                        .Where(r => r is not null)
                        .Where(r => !string.IsNullOrWhiteSpace(r.Family) || r.DatabaseFamilyId.HasValue)
                        .GroupBy(GetFamilyGroupKey, StringComparer.OrdinalIgnoreCase);

                    foreach (var g in familyGroups)
                    {
                        var rows = g.ToList();
                        if (rows.Count == 0) continue;

                        var familyDraft = CreateFamilyDraftFromGroup(rows);
                        working[familyDraft.Id] = familyDraft;
                    }

                    foreach (var row in analyzedRows)
                    {
                        //Draft типа компонента
                        var componentDraft = CreateDraftFromImportedRow(row);
                        working[componentDraft.Id] = componentDraft;
                    }
                }
                _current = _current with { WorkingComponents = working };

                MarkDirty_NoLock();

                sw.Stop();
                Debug.WriteLine($"[STORE] InitializeWorkingDrafts: {sw.ElapsedMilliseconds}ms (components: {working.Count})");

                args = BuildArgs_NoLock(StoreChangeKind.WorkingInitialized, key: "WORKING", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }

            Changed?.Invoke(this, args);
        }


        public IReadOnlyList<ComponentDraft> GetAllWorking()
        {
            _lock.EnterReadLock();
            try
            {
                EnsureIndexes_NoLock();
                return _draftsById.Values.ToList();
            }
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
                EnsureIndexes_NoLock();

                // O(1) lookup в индексе!
                return _draftsById.TryGetValue(draftId, out var draft)
                    ? draft
                    : null;
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

            Features.Workspace.Models.WorkspaceProject snapshot;
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

        public void AssignFamily(Guid componentDraftId, string? familyName, int? dbFamilyId)
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                if (_current.WorkingComponents is null || _current.WorkingComponents.Count == 0)
                    return;

                var working = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);

                if (!working.TryGetValue(componentDraftId, out var component))
                    return;

                if (component.Kind != DraftKind.Component)
                    return;

                var oldKey = component.FamilyKey;
                var newKey = BuildFamilyKey(familyName, dbFamilyId);

                //ничего не меняем
                if(string.Equals(oldKey, newKey, StringComparison.OrdinalIgnoreCase)
                    && string.Equals(component.Family, familyName, StringComparison.OrdinalIgnoreCase)
                    && component.DbFamilyId == dbFamilyId)
                {
                    return;
                }

                //1) обновляем компонент
                component = component with
                {
                    Family = familyName,
                    DbFamilyId = dbFamilyId,
                    FamilyKey = newKey
                };
                working[componentDraftId] = component;

                //2) пересчитываем семейные агрегаты
                RebuildFamilyAggregate_NoLock(working, oldKey);
                RebuildFamilyAggregate_NoLock(working, newKey);

                //3) обновляем проект
                _current = _current with { WorkingComponents = working };

                //4) пересобираем views
                RebuildViewsByForms_NoLock();

                MarkDirty_NoLock();
                args = BuildArgs_NoLock(StoreChangeKind.WorkingReplaced, key: "WORKING", draftIds: null);
            }
            finally { _lock.ExitWriteLock(); }
            Changed?.Invoke(this, args);
        }

        #endregion
       
        #region Utility

        public void Clear()
        {
            StoreChangedEventArgs args;

            _lock.EnterWriteLock();
            try
            {
                _current = Features.Workspace.Models.WorkspaceProject.Empty();
                _documentBinaries.Clear();

                InvalidateIndexes_NoLock();
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
        
        #endregion

        #region Index Management

        /// <summary>
        /// ОПТИМИЗАЦИЯ: Перестраивает все индексы за один проход
        /// Вызывается при загрузке проекта, инициализации, RebuildViews
        /// </summary>
        private void RebuildIndexes_NoLock()
        {
            var sw = Stopwatch.StartNew();

            _draftsByFormCode.Clear();
            _draftsById.Clear();
            _sortedFormCodes.Clear();

            var working = _current.WorkingComponents;
            if(working == null || working.Count == 0)
            {
                _indexesValid = true;
                return;
            }

            var formCodesSet = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Один проход во всем компонентам
            foreach(var draft in working.Values)
            {
                if (draft == null) continue;

                // Индекс по ID
                _draftsById[draft.Id] = draft;

                // Индекс по FormCode
                var formCode = string.IsNullOrWhiteSpace(draft.FormCode)
                    ? WorkspaceViewKeys.UndefinedForm
                    : draft.FormCode;

                if(!_draftsByFormCode.ContainsKey(formCode))
                    _draftsByFormCode[formCode] = new List<ComponentDraft>();

                _draftsByFormCode[formCode].Add(draft);
                formCodesSet.Add(formCode);
            }

            // Сортируем ключи форм
            _sortedFormCodes = formCodesSet
                .OrderBy(k => string.Equals(k, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(k => k, StringComparer.OrdinalIgnoreCase)
                .ToList();

            _indexesValid = true;

            sw.Stop();
            Debug.WriteLine($"[STORE] RebuildIndexes: {sw.ElapsedMilliseconds}ms (drafts: {working.Count}, forms: {formCodesSet.Count})");

        }

        /// <summary>
        /// Инвалидирует индексы (помечает как невалидные)
        /// </summary>
        private void InvalidateIndexes_NoLock()
        {
            _indexesValid = false;
        }

        /// <summary>
        /// Проверяет валидность индексов, перестраивает если нужно
        /// </summary>
        private void EnsureIndexes_NoLock()
        {
            if (!_indexesValid)
                RebuildIndexes_NoLock();
        }

        /// <summary>
        /// Обновляет один draft в индексах (при UpdateWorking)
        /// </summary>
        private void UpdateDraftInIndexes_NoLock(ComponentDraft draft)
        {
            if (!_indexesValid) return; // Будет пересоздано при следующем запросе

            // Обновляем в _draftsById
            _draftsById[draft.Id] = draft;

            // Обновляем в _draftsByFormCode
            var formCode = string.IsNullOrWhiteSpace(draft.FormCode)
                ? WorkspaceViewKeys.UndefinedForm
                : draft.FormCode;

            // Удаляем из всех форм (может был в другой)
            foreach (var list in _draftsByFormCode.Values)
            {
                list.RemoveAll(d => d.Id == draft.Id);
            }

            // Добавляем в правильную форму
            if (!_draftsByFormCode.ContainsKey(formCode))
                _draftsByFormCode[formCode] = new List<ComponentDraft>();

            _draftsByFormCode[formCode].Add(draft);
        }

        /// <summary>
        /// Добавляет draft в индексы (при Split/Merge)
        /// </summary>
        private void AddDraftToIndexes_NoLock(ComponentDraft draft)
        {
            if (!_indexesValid) return;

            _draftsById[draft.Id] = draft;

            var formCode = string.IsNullOrWhiteSpace(draft.FormCode)
                ? WorkspaceViewKeys.UndefinedForm
                : draft.FormCode;

            if (!_draftsByFormCode.ContainsKey(formCode))
                _draftsByFormCode[formCode] = new List<ComponentDraft>();

            _draftsByFormCode[formCode].Add(draft);
        }

        /// <summary>
        /// Удаляет draft из индексов (при Remove/Split/Merge)
        /// </summary>
        private void RemoveDraftFromIndexes_NoLock(Guid draftId)
        {
            if (!_indexesValid) return;

            _draftsById.Remove(draftId);

            foreach (var list in _draftsByFormCode.Values)
            {
                list.RemoveAll(d => d.Id == draftId);
            }
        }

        #endregion

        #region Private Helpers

        private void MarkDirty_NoLock() => _hasUnsavedChanges = true;

        private void RebuildViewsByForms_NoLock()
        {
            var working = _current.WorkingComponents;
            var views = new Dictionary<string, List<Guid>>(StringComparer.OrdinalIgnoreCase);

            if (working is not null && working.Count > 0)
            {
                // Undefined view (если FormCode пустой)
                var undefinedIds = working.Values
                    .Where(d => d is not null && string.IsNullOrWhiteSpace(d.FormCode))
                    .OrderBy(d => d.SourceRowId)
                    .ThenBy(d => d.Name, StringComparer.OrdinalIgnoreCase)
                    .Select(d => d.Id)
                    .ToList();

                if (undefinedIds.Count > 0)
                    views[WorkspaceViewKeys.UndefinedForm] = undefinedIds;

                var grouped = working.Values
                    .Where(d => d is not null && !string.IsNullOrWhiteSpace(d.FormCode))
                    .GroupBy(d => d.FormCode!, StringComparer.OrdinalIgnoreCase);

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
        }

        private ComponentDraft CreateDraftFromImportedRow(ImportedRow row)
        {
            var emptyParams = new Dictionary<int, ParameterValueDraft>();
            var designators = DesignatorHelper.Expand(row.Designator);

            var formCode = row.ComponentExistsInDatabase && !string.IsNullOrWhiteSpace(row.ComponentFormCode)
                ? row.ComponentFormCode!
                : WorkspaceViewKeys.UndefinedForm;

            var formName = row.ComponentExistsInDatabase
                ? (row.ComponentFormDisplayName ?? formCode)
                : string.Empty;

            var resolvedFormName = row.ComponentFormDisplayName ?? string.Empty;

            return new ComponentDraft(
                Id: Guid.NewGuid(),
                SourceRowId: row.RowId,

                Name: row.CleanName,
                DBComponentId: row.ExistingComponentId,

                Family: row.Family,
                DbFamilyId: row.DatabaseFamilyId,
                FamilyKey: GetFamilyGroupKey(row),

                DbComponentFormId: row.ComponentFormId,
                FormCode: formCode,
                FormName: formName,

                Quantity: row.Quantity,

                Kind: DraftKind.Component,

                Designators: designators,

                SelectedRemarksIds: Array.Empty<int>(),

                NdtParametersOverrides: emptyParams,
                ApprovalRef: MissingApproval,

                SchematicParameters: emptyParams,
                LocalFillStatus: LocalFillStatus.Missing
                );
        }

        private ComponentDraft CreateFamilyDraftFromGroup(List<ImportedRow> rows)
        {
            if (rows is null) throw new ArgumentNullException(nameof(rows));
            if (rows.Count == 0) throw new ArgumentException("Group is empty.", nameof(rows));

            var main = rows[0];

            var formCode = WorkspaceViewKeys.FamilyFormCode;
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

                Family: familyName,
                DbFamilyId: main.DatabaseFamilyId,
                FamilyKey: GetFamilyGroupKey(main),

                DbComponentFormId: null,
                FormCode: formCode,
                FormName: formName,

                Quantity: quantity,

                Kind: DraftKind.FamilyAgregate,

                Designators: mergeDesignators,

                SelectedRemarksIds: Array.Empty<int>(),

                NdtParametersOverrides: emptyParams,
                ApprovalRef: MissingApproval,
                SchematicParameters: emptyParams,
                LocalFillStatus: LocalFillStatus.Missing
                );
        }

        private StoreChangedEventArgs BuildArgs_NoLock(StoreChangeKind kind, string? key, IReadOnlyList<Guid>? draftIds)
        {
            var totalWorking = _current.WorkingComponents?.Count ?? 0;
            return new StoreChangedEventArgs(kind, key, totalWorking, draftIds);
        }

        private void RebuildFamilyAggregate_NoLock(Dictionary<Guid, ComponentDraft> working, string familyKey)
        {
            if (string.IsNullOrWhiteSpace(familyKey))
                return;

            if (string.Equals(familyKey, WorkspaceViewKeys.NoFamily, StringComparison.OrdinalIgnoreCase))
            {
                var stray = working.Values.FirstOrDefault(d =>
                d.Kind == DraftKind.FamilyAgregate &&
                string.Equals(d.FamilyKey, familyKey, StringComparison.OrdinalIgnoreCase));

                if (stray is not null)
                    working.Remove(stray.Id);

                return;
            }

            //Все компоненты этого семейства
            var components = working.Values
                .Where(d => d.Kind == DraftKind.Component
                        && string.Equals(d.FamilyKey, familyKey, StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Существющий агрегат
            var existingFamily = working.Values.FirstOrDefault(d =>
                d.Kind == DraftKind.FamilyAgregate &&
                string.Equals(d.FamilyKey, familyKey, StringComparison.OrdinalIgnoreCase));

            //Если компонентов больше нет - удалить агрегат
            if (components.Count == 0)
            {
                if (existingFamily is not null)
                    working.Remove(existingFamily.Id);

                return;
            }

            var mergedDesignators = components
                .SelectMany(c => c.Designators ?? Array.Empty<string>())
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .Select(d => d.Trim())
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            var quantitySum = components.Sum(c => c.Quantity);
            var quantity = mergedDesignators.Count > 0 ? mergedDesignators.Count : quantitySum;

            var first = components[0];
            var resolvedFamilyname = first.Family;
            var resolvedDbFamilyId = first.DbFamilyId;

            // Имя семейного агрегата: Family, иначе Name первого компонента
            var name = !string.IsNullOrWhiteSpace(resolvedFamilyname) ? resolvedFamilyname! : first.Name;

            if (existingFamily is not null)
            {
                //Обновляем только агригируемые поля, параметры не трогаем
                var update = existingFamily with
                {
                    SourceRowId = first.SourceRowId,
                    Name = name,

                    Family = resolvedFamilyname,
                    DbFamilyId = resolvedDbFamilyId,
                    FamilyKey = familyKey,

                    DbComponentFormId = null,
                    FormCode = WorkspaceViewKeys.FamilyFormCode,
                    FormName = WorkspaceViewKeys.FamilyFormName,

                    Quantity = quantity,
                    Designators = mergedDesignators,
                    Kind = DraftKind.FamilyAgregate,
                };

                working[update.Id] = update;
                return;
            }

            //Если агрегата не было - создаём новый (параметры пустые)
            var emptyParams = new Dictionary<int, ParameterValueDraft>();

            var created = new ComponentDraft(
                Id: Guid.NewGuid(),
                SourceRowId: first.SourceRowId,

                Name: name,
                DBComponentId: null,

                Family: resolvedFamilyname,
                DbFamilyId: resolvedDbFamilyId,
                FamilyKey: familyKey,

                DbComponentFormId: null,
                FormCode: WorkspaceViewKeys.FamilyFormCode,
                FormName: WorkspaceViewKeys.FamilyFormName,

                Quantity: quantity,
                Kind: DraftKind.FamilyAgregate,

                Designators: mergedDesignators,
                SelectedRemarksIds: Array.Empty<int>(),

                NdtParametersOverrides: emptyParams,
                ApprovalRef: MissingApproval,

                SchematicParameters: emptyParams,
                LocalFillStatus: LocalFillStatus.Missing
                );

            working[created.Id] = created;
        }

        private static string GetFamilyGroupKey(ImportedRow r)
        {
            if (r.DatabaseFamilyId is int id && id > 0)
                return $"DBF:{id}";

            if (!string.IsNullOrWhiteSpace(r.Family))
                return $"FAM:{NormalizeKey(r.Family)}";

            return WorkspaceViewKeys.NoFamily;
        }

        private static string NormalizeKey(string s)
        {
            s = s.Trim().ToUpperInvariant();
            return string.Join(
                ' ',
                s.Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        private string BuildFamilyKey(string? familyName, int? dbFamilyId)
        {
            if (dbFamilyId is int id && id > 0)
                return $"DBF:{id}";

            if (!string.IsNullOrWhiteSpace(familyName))
                return $"FAM:{NormalizeKey(familyName)}";

            return WorkspaceViewKeys.NoFamily;
        }
        
        #endregion

    }
}

