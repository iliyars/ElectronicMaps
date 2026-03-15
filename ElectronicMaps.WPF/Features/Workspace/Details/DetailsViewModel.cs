using Azure.Core.Extensions;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DocumentFormat.OpenXml.Drawing;
using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Application.Features.Workspace.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Features.Workspace.Parameters;
using ElectronicMaps.WPF.Services.Dialogs;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Features.Workspace.Details
{
    /// <summary>
    /// Пара строк для одного параметра:
    ///   Ndt      — значение из БД (readonly, эталон)
    ///   Schematic — значение в схеме (редактируемое)
    /// </summary>
    public record ParameterRowPair(
        ParameterValueRowViewModel Ndt,
        ParameterValueRowViewModel Schematic);

    /// <summary>
    /// ViewModel панели деталей компонента.
    ///
    /// Два набора параметров рядом:
    ///   • NdtRows        — значения из БД (по даташиту), только для чтения
    ///   • SchematicRows  — значения в текущей схеме, редактируемые пользователем
    ///
    /// SchematicParameters НЕ пишутся в БД — они сохраняются в файл проекта
    /// через store.SaveProjectAsync (когда пользователь сохраняет проект).
    /// </summary>
    public partial class DetailsViewModel : ObservableObject
    {

        private readonly IComponentStore _componentStore;
        private readonly IWorkspaceActions _workspaceActions;
        private readonly IDialogService _dialogService;
        private readonly ILogger<DetailsViewModel> _logger;

        private Guid _currentDraftId = Guid.Empty;


        #region Observable свойства

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReady))]
        private string componentName = string.Empty;

        [ObservableProperty]
        private string formName = string.Empty;

        [ObservableProperty]
        private DraftKind draftKind;

        [ObservableProperty]
        [NotifyPropertyChangedFor(nameof(IsReady))]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DiscardCommand))]
        private bool isBusy;

        [ObservableProperty]
        private string? statusMessage;


        /// <summary>
        /// Есть ли несохранённые (не применённые в store) изменения.
        /// Сброс происходит при Apply или Discard.
        /// Проект при этом помечается dirty автоматически через store.
        /// </summary>
        [ObservableProperty]
        [NotifyCanExecuteChangedFor(nameof(ApplyCommand))]
        [NotifyCanExecuteChangedFor(nameof(DiscardCommand))]
        private bool isDirty;

     

        /// <summary>Панель готова к отображению.</summary>
        public bool IsReady => !IsBusy && _currentDraftId != Guid.Empty;

        /// <summary>
        /// Параметры из БД (по даташиту) — только для чтения.
        /// </summary>
        public ObservableCollection<ParameterValueRowViewModel> NdtRows { get; } = new();

        /// <summary>
        /// Параметры в текущей схеме — редактируемые.
        /// Синхронизированы по индексу с NdtRows.
        /// </summary>
        public ObservableCollection<ParameterValueRowViewModel> SchematicRows { get; } = new();

        public ObservableCollection<ParameterRowPair> ParameterRows { get; } = new();
        #endregion

        public DetailsViewModel(
            IComponentStore componentStore,
            IWorkspaceActions workspaceActions,
            IDialogService dialogService,
            ILogger<DetailsViewModel> logger)
        {
            _componentStore = componentStore;
            _workspaceActions = workspaceActions;
            _dialogService = dialogService;
            _logger = logger;
        }

        /// <summary>
        /// Загружает детали компонента.
        /// Вызывать из WorkspaceViewModel при открытии панели или смене карточки.
        /// </summary>
        public async Task LoadAsync(ComponentDraft draft, CancellationToken ct = default)
        {
            if(draft == null) throw new ArgumentNullException(nameof(draft));

            if (_currentDraftId == draft.Id && SchematicRows.Count > 0)
                return;

            _currentDraftId = draft.Id;
            IsBusy = true;
            isDirty = false;
            statusMessage = "Загрузка параметров...";

            try
            {
                ComponentName = draft.Name;
                FormName = draft.FormName;
                DraftKind = draft.Kind;

                var definitions = await _workspaceActions.LoadDefinitionsForDraftAsync(draft.Id, ct);
                RebuildRows(draft, definitions);

                StatusMessage = SchematicRows.Count > 0
                    ? $"параметров: {SchematicRows.Count}"
                    : "Нет параметров для это формы";
            }
            catch (OperationCanceledException)
            {
                StatusMessage = null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка загрузки параметров для '{Name}'", draft.Name);
                StatusMessage = $"Ошибка загрузки: {ex.Message}";
                await _dialogService.ShowErrorAsync("Ошибка", $"Не удалось загрузить параметры:\n{ex.Message}");
            }
            finally
            {
                IsBusy = false;
                OnPropertyChanged(nameof(IsBusy));
            }
        }


        /// <summary>
        /// Обновляет отображение при изменении draft в store извне
        /// (например, после AssignFamily или реимпорта XML).
        /// НДТ-колонка пересчитывается всегда.
        /// Schematic-колонка не трогается если есть несохранённые изменения.
        /// </summary>
        public void RefreshFromDraft(ComponentDraft draft)
        {
            if (draft.Id != _currentDraftId) return;

            ComponentName = draft.Name;
            FormName = draft.FormName;
            DraftKind = draft.Kind;

            foreach(var ndtRow in NdtRows)
            {
                if (draft.NdtParametersOverrides.TryGetValue(ndtRow.DefinitionId, out var ndtDraft))
                    FillRowFromDraft(ndtRow, ndtDraft);
                else
                    ndtRow.ClearValue();
            }

            if(!isDirty)
            {
                foreach(var schRow in SchematicRows)
                {
                    if (draft.SchematicParameters.TryGetValue(schRow.DefinitionId, out var schDraft))
                        FillRowFromDraft(schRow, schDraft);
                    else
                        schRow.ClearValue();
                }
            }
        }

        /// <summary>
        /// Сбрасывает панель при её закрытии.
        /// </summary>
        public void Clear()
        {
            UnsubscribeRows();

            _currentDraftId = Guid.Empty;
            ComponentName = string.Empty;
            FormName = string.Empty;
            StatusMessage = null;
            IsDirty = false;
            NdtRows.Clear();
            SchematicRows.Clear();

            OnPropertyChanged(nameof(IsReady));
        }


        #region Команды


        /// <summary>
        /// Применяет SchematicParameters в store.
        /// В БД ничего не пишется — данные сохранятся в файл проекта при его сохранении.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanApply))]
        private void Apply()
        {
            if (_currentDraftId == Guid.Empty) return;

            try
            {
                var newSchematicParams = BuildSchematicDictionary();

                var updated = _componentStore.UpdateWorking(_currentDraftId, draft => draft with
                {
                    SchematicParameters = newSchematicParams,
                    LocalFillStatus = newSchematicParams.Count > 0
                        ? LocalFillStatus.Filled
                        : LocalFillStatus.Missing
                });

                if (!updated)
                {
                    _logger.LogWarning("Draft {Id} не найден в store при применении", _currentDraftId);
                    _dialogService.ShowMessage("Компонент не найден в рабочем наборе.", "Ошибка");
                    return;
                }

                IsDirty = false;
                StatusMessage = "Применено";
                _logger.LogInformation(
                    "Параметры схемы '{Name}' применены в store ({Count} шт.)",
                    ComponentName, newSchematicParams.Count);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка применения параметров '{Name}'", ComponentName);
                StatusMessage = $"Ошибка: {ex.Message}";
                _dialogService.ShowMessage($"Не удалось применить параметры:\n{ex.Message}", "Ошибка");
            }
        }

        private bool CanApply() => IsDirty && !IsBusy;


        /// <summary>
        /// Откатывает несохранённые изменения из текущего состояния store.
        /// </summary>
        [RelayCommand(CanExecute = nameof(CanDiscard))]
        private void Discard()
        {
            if (_currentDraftId == Guid.Empty) return;

            var draft = _componentStore.TryGetWorking(_currentDraftId);
            if (draft == null) return;

            foreach (var schRow in SchematicRows)
            {
                if (draft.SchematicParameters.TryGetValue(schRow.DefinitionId, out var saved))
                    FillRowFromDraft(schRow, saved);
                else
                    schRow.ClearValue();
            }

            IsDirty = false;
            StatusMessage = "Изменения отменены";
        }

        private bool CanDiscard() => IsDirty && !IsBusy;

        #endregion


        #region Private Methods

        private void RebuildRows(ComponentDraft draft, IReadOnlyList<ParameterDefinitionDto> definitions)
        {
            UnsubscribeRows();
            NdtRows.Clear();
            SchematicRows.Clear();

            foreach (var def in definitions.OrderBy(d => d.Order))
            {
                var ndtRow = new ParameterValueRowViewModel(def);
                if (draft.NdtParametersOverrides.TryGetValue(def.Id, out var ndtDraft))
                    FillRowFromDraft(ndtRow, ndtDraft);
                NdtRows.Add(ndtRow);

                var schRow = new ParameterValueRowViewModel(def);
                if (draft.SchematicParameters.TryGetValue(def.Id, out var schDraft))
                    FillRowFromDraft(schRow, schDraft);
                schRow.PropertyChanged += OnSchematicRowChanged;
                SchematicRows.Add(schRow);
            }
            ParameterRows.Clear();
            for(int i = 0; i < NdtRows.Count; i++)
            {
                ParameterRows.Add(new ParameterRowPair(NdtRows[i], SchematicRows[i]));
            }
        }

        private void UnsubscribeRows()
        {
            foreach (var row in SchematicRows)
                row.PropertyChanged -= OnSchematicRowChanged;
        }

        private void OnSchematicRowChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (!IsDirty)
            {
                IsDirty = true;
                _logger.LogDebug("Изменён параметр схемы '{Name}'", ComponentName);
            }
        }

        private static void FillRowFromDraft(ParameterValueRowViewModel row, ParameterValueDraft draft)
        {
            row.StringValue = draft.StringValue;
            row.DoubleValue = draft.DoubleValue;
            row.IntValue = draft.IntValue;
            row.Pins = draft.Pins;
        }

        private IReadOnlyDictionary<int, ParameterValueDraft> BuildSchematicDictionary()
        {
            var result = new Dictionary<int, ParameterValueDraft>();

            foreach (var row in SchematicRows)
            {
                if (!row.HasValue) continue;

                result[row.DefinitionId] = new ParameterValueDraft(
                    ParameterDefinitionId: row.DefinitionId,
                    Code: row.Code,
                    DisplayName: row.DisplayName,
                    Unit: row.Unit,
                    StringValue: row.StringValue,
                    DoubleValue: row.DoubleValue,
                    IntValue: row.IntValue,
                    Pins: row.Pins
                );
            }

            return result;
        }

        #endregion
    }
}
