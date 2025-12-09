using ElectronicMaps.Application.DTO;
using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.IO;
using System.Threading;

namespace ElectronicMaps.Application.Stores
{
    public sealed class ComponentStore : IComponentStore
    {
        private readonly ConcurrentDictionary<string, List<AnalyzedComponentDto>> _map = new(StringComparer.OrdinalIgnoreCase);

        private readonly ReaderWriterLockSlim _lock = new();

        private readonly IComponentStoreSerializer _serializer;

        private readonly string _defaultPath;

        private bool _hasUnsavedChanges;
        public bool HasUnsavedChanges => _hasUnsavedChanges;

        // Используется только для Clone
        private static readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        };
        /// <inheritdoc />
        public event EventHandler<StoreChangedEventArgs>? Changed;

        /// <summary>
        /// Создаёт экземпляр хранилища компонентов.
        /// </summary>
        /// <param name="serializer">
        /// Сериализатор для сохранения/загрузки данных в файл.
        /// </param>
        public ComponentStore(IComponentStoreSerializer serializer)
        {
            _serializer = serializer ?? throw new ArgumentNullException(nameof(serializer));
            _defaultPath = Path.Combine(Path.GetTempPath(), "components.json");
            _hasUnsavedChanges = false;
        }
        ///<inheritdoc/>
        public IReadOnlyList<AnalyzedComponentDto> GetAll()
        {
            _lock.EnterReadLock();


            try
            {
                if (_map.TryGetValue("ALL", out var list))
                    return list.Select(Clone).ToList();

                return Array.Empty<AnalyzedComponentDto>();
            }
            finally
            {
                _lock.ExitReadLock();
            }

        }

        ///<inheritdoc/>
        public IReadOnlyList<AnalyzedComponentDto> GetByComponentForm(string formCode)
        {
            if (string.IsNullOrWhiteSpace(formCode))
            {
                return Array.Empty<AnalyzedComponentDto>();
            }

            _lock.EnterReadLock();
            try
            {
                if (_map.TryGetValue(formCode, out var list))
                    return list.Select(Clone).ToList();

                return Array.Empty<AnalyzedComponentDto>();
            }
            finally
            {
                _lock.ExitReadLock();
            }
        }

        ///<inheritdoc/>
        public void ReplaceAll(IEnumerable<AnalyzedComponentDto> components)
        {
            if (components is null)
                throw new ArgumentNullException(nameof(components));

            _lock.EnterWriteLock();
            try
            {
                _map.Clear();

                var all = components
                    .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.CleanName))
                    .Select(Clone)
                    .ToList();

                _map["ALL"] = all;

                OnChanged(StoreChangeKind.Replaced, "ALL");
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        //<inheritdoc/>
        public void ReplaceForm(string formCode, IEnumerable<AnalyzedComponentDto> components)
        {
            if (string.IsNullOrWhiteSpace(formCode))
                throw new ArgumentException("Код формы не должен быть пустым.", nameof(formCode));

            if (components is null)
                throw new ArgumentNullException(nameof(components));

            var list = components
                .Where(c => c is not null && !string.IsNullOrWhiteSpace(c.CleanName))
                .Select(Clone)
                .ToList();

            _lock.EnterWriteLock();
            try
            {
                _map[formCode] = list;
                OnChanged(StoreChangeKind.Replaced, formCode);
            }
            finally
            {
                _lock.ExitWriteLock();
            }

        }
        ///<inheritdoc/>
        public void AddToForm(string formCode, AnalyzedComponentDto component)
        {
            if (string.IsNullOrWhiteSpace(formCode))
                throw new ArgumentException("Код формы не должен быть пустым.", nameof(formCode));

            if (component is null)
                throw new ArgumentNullException(nameof(component));

            var clone = Clone(component);

            _lock.EnterWriteLock();
            try
            {
                var list = _map.GetOrAdd(formCode, _ => new List<AnalyzedComponentDto>());
                list.Add(clone);

                OnChanged(StoreChangeKind.Upserted, formCode);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        ///<inheritdoc/>
        public void RemoveFromForm(string formCode, string cleanName)
        {
            if (string.IsNullOrWhiteSpace(formCode))
                throw new ArgumentException("Код формы не должен быть пустым.", nameof(formCode));

            if (string.IsNullOrWhiteSpace(cleanName))
                return;

            _lock.EnterWriteLock();
            try
            {
                if (_map.TryGetValue(formCode, out var list))
                {
                    var before = list.Count;
                    list.RemoveAll(c =>
                        c.CleanName.Equals(cleanName, StringComparison.OrdinalIgnoreCase));

                    if (list.Count != before)
                        OnChanged(StoreChangeKind.Removed, formCode);
                }
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        ///<inheritdoc/>
        public bool Remove(string key)
        {
            if (string.IsNullOrWhiteSpace(key))
                return false;

            bool removed = false;

            _lock.EnterWriteLock();
            try
            {
                foreach (var kv in _map)
                {
                    var list = kv.Value;
                    var before = list.Count;

                    list.RemoveAll(c =>
                        c.CleanName.Equals(key, StringComparison.OrdinalIgnoreCase));

                    if (list.Count != before)
                        removed = true;
                }

                if (removed)
                    OnChanged(StoreChangeKind.Removed, key);

                return removed;
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }
        /// <inheritdoc />
        public void Clear()
        {
            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
                OnChanged(StoreChangeKind.Cleared, null);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }

        public void MarkClean()
        {
            _hasUnsavedChanges = false;
        }

        ///<inheritdoc/>
        public async Task SaveAsync(string? path = null, CancellationToken ct = default)
        {
            path ??= _defaultPath;

            Dictionary<string, List<AnalyzedComponentDto>> snapshot;

            _lock.EnterReadLock();
            try
            {
                snapshot = _map
                .ToDictionary(
                    kv => kv.Key,
                    kv => kv.Value.Select(Clone).ToList(),
                    StringComparer.OrdinalIgnoreCase);
            }
            finally
            {
                _lock.ExitReadLock();
            }

            Directory.CreateDirectory(Path.GetDirectoryName(path)!);

            await using var stream = File.Create(path);
            await _serializer.SerializeAsync(stream, snapshot, ct);

            OnChanged(StoreChangeKind.Saved, null);
        }

        ///<inheritdoc/>
        public async Task LoadAsync(string? path = null, CancellationToken ct = default)
        {
            path ??= _defaultPath;

            if (!File.Exists(path))
                return;

            await using var stream = File.OpenRead(path);
            var loaded = await _serializer.DeserializeAsync(stream, ct);

            _lock.EnterWriteLock();
            try
            {
                _map.Clear();
                foreach (var kv in loaded)
                {
                    _map[kv.Key] = kv.Value
                        ?.Where(x => x != null && !string.IsNullOrWhiteSpace(x.CleanName))
                        .Select(Clone)
                        .ToList()
                        ?? new List<AnalyzedComponentDto>();
                }

                OnChanged(StoreChangeKind.Loaded, null);
            }
            finally
            {
                _lock.ExitWriteLock();
            }
        }


        public void Dispose()
        {
            _lock.Dispose();
        }

        private static AnalyzedComponentDto Clone(AnalyzedComponentDto source)
        {
            var json = JsonSerializer.Serialize(source, _jsonOptions);
            return JsonSerializer.Deserialize<AnalyzedComponentDto>(json, _jsonOptions)
                   ?? throw new InvalidOperationException("Не удалось клонировать AnalyzedComponentDto.");
        }

        private void OnChanged(StoreChangeKind kind, string? key)
        {

            var handler = Changed;
            if (handler is null)
            {
                return;
            }

            var count = _map.Count;
            var args = new StoreChangedEventArgs(kind, key, count);
            handler(this, args);
        }
    }
}
