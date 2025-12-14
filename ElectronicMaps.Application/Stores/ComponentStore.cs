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
        private readonly ReaderWriterLockSlim _lock = new();
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


        // ---------------- Import (ALL) ----------------
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
                OnChanged(StoreChangeKind.Replaced, "ALL");
            }
            finally { _lock.ExitWriteLock(); }
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
                OnChanged(StoreChangeKind.Replaced, key);
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveView(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            _lock.EnterWriteLock();
            try
            {
                if (!_current.ViewsByKey.ContainsKey(key))
                    return false;

                var newViews = new Dictionary<string, List<Guid>>(_current.ViewsByKey, StringComparer.OrdinalIgnoreCase);
                var removed = newViews.Remove(key);
                if (!removed) return false;

                _current = _current with { ViewsByKey = newViews };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Removed, key);
                return true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        // ---------------- Working components ----------------
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

            _lock.EnterWriteLock();
            try
            {
                _current = _current with { WorkingComponents = dict };
                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Replaced, "WORKING");
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void UpsertWorking(ComponentDraft component)
        {
            if (component.Id == Guid.Empty)
                throw new ArgumentException("ComponentDraft.Id is empty.", nameof(component));

            _lock.EnterWriteLock();
            try
            {
                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                dict[component.Id] = component;

                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Upserted, component.Id.ToString());
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveWorking(Guid id)
        {
            if (id == Guid.Empty)
                return false;

            _lock.EnterWriteLock();
            try
            {
                if (!_current.WorkingComponents.ContainsKey(id))
                    return false;

                var dict = new Dictionary<Guid, ComponentDraft>(_current.WorkingComponents);
                var removed = dict.Remove(id);
                if (!removed) return false;

                _current = _current with { WorkingComponents = dict };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Removed, id.ToString());
                return true;
            }
            finally { _lock.ExitWriteLock(); }
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

            _lock.EnterWriteLock();
            try
            {
                var list = _current.Documents.ToList();
                list.Add(doc);

                _current = _current with { Documents = list };

                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Upserted, doc.DocumentId.ToString());
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveDocument(Guid documentId)
        {
            if (documentId == Guid.Empty)
                return false;

            _lock.EnterWriteLock();
            try
            {
                var list = _current.Documents.ToList();
                var removed = list.RemoveAll(d => d.DocumentId == documentId) > 0;
                if (!removed) return false;

                _current = _current with { Documents = list };

                _documentBinaries.Remove(documentId);
                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Removed, documentId.ToString());
                return true;
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void AddOrReplaceDocumentBinary(Guid documentId, byte[] content)
        {
            if (documentId == Guid.Empty) throw new ArgumentException("DocumentId is empty.", nameof(documentId));
            if (content is null || content.Length == 0) throw new ArgumentException("Content is empty.", nameof(content));

            _lock.EnterWriteLock();
            try
            {
                _documentBinaries[documentId] = content;
                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Upserted, documentId.ToString());
            }
            finally { _lock.ExitWriteLock(); }
        }

        public bool RemoveDocumentBinary(Guid documentId)
        {
            if (documentId == Guid.Empty) return false;

            _lock.EnterWriteLock();
            try
            {
                var removed = _documentBinaries.Remove(documentId);
                if (removed)
                {
                    MarkDirty_NoLock();
                    OnChanged(StoreChangeKind.Removed, documentId.ToString());
                }
                return removed;
            }
            finally { _lock.ExitWriteLock(); }
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
            await _serializer.SaveAsync(filePath, snapshot, docs, ct);

            _hasUnsavedChanges = false;
            OnChanged(StoreChangeKind.Saved, null);
        }

        public async Task LoadProjectAsync(string filePath, CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(filePath))
                throw new ArgumentException("File path is empty.", nameof(filePath));

            var loaded = await _serializer.LoadAsync(filePath, ct);

            _lock.EnterWriteLock();
            try
            {
                _current = loaded.Project;

                _documentBinaries = loaded.Documents.GroupBy(d => d.DocumentId).ToDictionary(g => g.Key, g => g.Last().Content);

                _hasUnsavedChanges = false;
                OnChanged(StoreChangeKind.Loaded, null);
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _current = WorkspaceProject.Models.WorkspaceProject.Empty();
                MarkDirty_NoLock();
                OnChanged(StoreChangeKind.Cleared, null);
            }
            finally { _lock.ExitWriteLock(); }
        }

        public void MarkClean() => _hasUnsavedChanges = false;

        public void Dispose() => _lock.Dispose();

        private void MarkDirty_NoLock() => _hasUnsavedChanges = true;

        private void OnChanged(StoreChangeKind kind, string? key)
            => Changed?.Invoke(this, new StoreChangedEventArgs(kind, key, _current.ImportedRows.Count));

       
    }
}

