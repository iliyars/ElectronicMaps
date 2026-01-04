using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.FormCards;
using ElectronicMaps.WPF.Features.Workspace.GridRows;
using ElectronicMaps.WPF.Infrastructure.ViewModels;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text;
using System.Windows;
using System.Windows.Data;
using System.Windows.Threading;
using Wpf.Ui.Abstractions.Controls;

namespace ElectronicMaps.WPF.Features.Workspace
{
    public record ViewOption(string Key, string Title)
    {
        public override string ToString() => Title;
    }

    public partial class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo, INavigatedFrom
    {
        private readonly IComponentStore _componentStore;
        private bool _subscribed;
        private bool _isInitializing;

        public ObservableCollection<ViewOption> ViewOptions { get; } = new();
        public ObservableCollection<CardViewModelBase> ItemCards { get; } = new();

        /// <summary>
        /// Все компоненты из xml.
        /// </summary>
        public ObservableCollection<ImportedRowViewModel> ImportedComponents { get; } = new();

        [ObservableProperty]
        private bool isDetailsOpen;

        [ObservableProperty]
        private ViewOption? selectedViewOption;

        [ObservableProperty]
        private Guid? openDetailsDraftId;

        [ObservableProperty]
        private CardViewModelBase? selectedCard;

        partial void OnSelectedViewOptionChanged(ViewOption? value)
        {
            if (_isInitializing)
                return;

            RebuildItemCards();
            //MessageBox.Show("OnSelectedViewKeyChanged");
        }

        public IRelayCommand<Guid> ToggleDetailsCommand { get; }
        public WorkspaceViewModel(IComponentStore componentStore)
        {
            _componentStore = componentStore;
            ToggleDetailsCommand = new RelayCommand<Guid>(ToggleDetails);
            // ОПТИМИЗАЦИЯ: Отключаем уведомления об изменениях коллекции во время массовых операций
            EnableCollectionSynchronization(ItemCards);
        }

        public Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default)
        {
            if (!_subscribed)
            {
                _componentStore.Changed += OnStoreChanged;
                _subscribed = true;
            }

            RebuildCards();
            SyncDetailsState();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync(CancellationToken cancellationToken = default)
        {
            if (_subscribed)
            {
                _componentStore.Changed -= OnStoreChanged;
                _subscribed = false;
            }
            return Task.CompletedTask;
        }

        private void ToggleDetails(Guid draftId)
        {
            if (OpenDetailsDraftId == draftId)
            {
                // Закрываем панель
                OpenDetailsDraftId = null;
                SelectedCard = null;
                IsDetailsOpen = false;
            }
            else
            {
                // Открываем панель
                OpenDetailsDraftId = draftId;
                SelectedCard = ItemCards.FirstOrDefault(c => c.Item.Id == draftId);
                IsDetailsOpen = true;
            }
            SyncDetailsState();
        }

        private void OnStoreChanged(object? sender, StoreChangedEventArgs e)
        {
            // ВАЖНО: если store меняется из background (БД/файлы), обновляем UI через Dispatcher
            if (!System.Windows.Application.Current.Dispatcher.CheckAccess())
            {
                System.Windows.Application.Current.Dispatcher.Invoke(() => OnStoreChanged(sender, e));
                return;
            }


            switch (e.Kind)
            {
                // Эти случаи проще пересобрать целиком (редко происходят)
                case StoreChangeKind.ProjectLoaded:
                case StoreChangeKind.WorkingInitialized:
                case StoreChangeKind.WorkingReplaced:
                case StoreChangeKind.ViewsRebuilt:
                    RebuildCards();
                    SyncDetailsState();
                    break;

                case StoreChangeKind.WorkingUpserted:
                    foreach (var id in e.DraftIds)
                        UpsertCard(id);
                    SyncDetailsState();
                    break;

                case StoreChangeKind.WorkingRemoved:
                    foreach (var id in e.DraftIds)
                        RemoveCard(id);
                    SyncDetailsState();
                    break;
            }
        }

        private void SyncDetailsState()
        {
            foreach (var card in ItemCards)
            {
                card.IsDetailsOpen = OpenDetailsDraftId.HasValue && card.Item.Id == OpenDetailsDraftId.Value;
            }
        }

        private void RebuildItemCards()
        {

            var totalSw = Stopwatch.StartNew();

            ItemCards.Clear();

            var key = SelectedViewOption?.Key ?? WorkspaceViewKeys.UndefinedForm;
            var storeSw = Stopwatch.StartNew();
            var drafts = _componentStore.GetWorkingForView(key);
            storeSw.Stop();

            var vmSw = Stopwatch.StartNew();
            var newCards = new List<CardViewModelBase>(drafts.Count);
            var number = 1;

            foreach (var draft in drafts)
                newCards.Add(CreateCardViewModel(draft, number++));
            vmSw.Stop();

            var collSw = Stopwatch.StartNew();
            ReplaceCollection(ItemCards, newCards);
            collSw.Stop();

            totalSw.Stop();

            Debug.WriteLine($"[PERF] ==========================================");
            Debug.WriteLine($"[PERF] Store:      {storeSw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[PERF] ViewModels: {vmSw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[PERF] Collection: {collSw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[PERF] TOTAL:      {totalSw.ElapsedMilliseconds}ms");
            Debug.WriteLine($"[PERF] ==========================================");

            // Измеряем UI rendering (отложенное)
            Dispatcher.CurrentDispatcher.BeginInvoke(new Action(() =>
            {
                var uiSw = Stopwatch.StartNew();
                System.Windows.Application.Current.MainWindow?.UpdateLayout();
                uiSw.Stop();
                Debug.WriteLine($"[PERF] UI Render:  {uiSw.ElapsedMilliseconds}ms");
            }), DispatcherPriority.Background);

            // Если была открыта панель деталей, но компонент исчез - закрываем
            if (OpenDetailsDraftId.HasValue)
            {
                var stillExists = ItemCards.Any(c => c.Item.Id == OpenDetailsDraftId.Value);
                if (!stillExists)
                {
                    OpenDetailsDraftId = null;
                    SelectedCard = null;
                    IsDetailsOpen = false;
                }
            }
        }

        private void RebuildCards()
        {
            try
            {
                _isInitializing = true;
                RebuildViewOptions();
            }
            finally
            {
                _isInitializing = false;
            }

            RebuildItemCards();
        }

        private void UpsertCard(Guid id)
        {
            var draft = _componentStore.TryGetWorking(id);
            if (draft == null) return;

            // Проверяем, относится ли этот draft к текущему view
            var currentViewKey = SelectedViewOption?.Key ?? WorkspaceViewKeys.UndefinedForm;
            if (!string.Equals(draft.FormCode, currentViewKey, StringComparison.OrdinalIgnoreCase))
            {
                // Этот компонент не для текущей формы, игнорируем
                return;
            }

            var existing = ItemCards.FirstOrDefault(c => c.Item.Id == id);
            if (existing is not null)
            {
                // Обновляем существующую карточку
                existing.ReplaceItem(draft);

                // Если это открытая карточка - обновляем SelectedCard
                if (SelectedCard?.Item.Id == id)
                {
                    SelectedCard = existing;
                }
                return;
            }

            // Если карточки ещё нет — добавляем
            var number = ItemCards.Count + 1;
            var newCard = CreateCardViewModel(draft, number);
            ItemCards.Add(newCard);
        }

        private void RemoveCard(Guid id)
        {
            var existing = ItemCards.FirstOrDefault(c => c.Item.Id == id);
            if (existing == null) return;

            ItemCards.Remove(existing);

            for (int i = 0; i < ItemCards.Count; i++)
            {
                ItemCards[i].Number = i + 1;
            }

            // если удалили открытую карточку — закрываем детали
            if (OpenDetailsDraftId == id)
            {
                OpenDetailsDraftId = null;
                SelectedCard = null;
                IsDetailsOpen = false;
            }
        }

        private void RebuildViewOptions()
        {
            var currentKey = SelectedViewOption?.Key;

            ViewOptions.Clear();

            var keys = _componentStore.GetViewKeys();
            if (!keys.Any())
            {
                // Нет доступных форм
                SelectedViewOption = null;
                return;
            }

            var ordered = keys
                .OrderBy(k => string.Equals(k, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase) ? 0 : 1)
                .ThenBy(k => ParseFormNumber(k) ?? int.MaxValue)
                .ThenBy(k => k, StringComparer.OrdinalIgnoreCase);

            foreach (var key in ordered)
            {
                ViewOptions.Add(new ViewOption(key, ToTitle(key)));
            }

            // Восстанавливаем выбор, если он ещё валиден
            if (!string.IsNullOrWhiteSpace(currentKey))
            {
                var matchingOption = ViewOptions.FirstOrDefault(x => x.Key == currentKey);
                if(matchingOption != null)
                {
                    SelectedViewOption = matchingOption;
                    return;
                }
            }
            SelectedViewOption = ViewOptions.FirstOrDefault();
        }

        private static int? ParseFormNumber(string key)
        {
            if (key.StartsWith("FORM_", StringComparison.OrdinalIgnoreCase))
            {
                var tail = key.Substring("FORM_".Length);
                if (int.TryParse(tail, out var n))
                    return n;
            }
            return null;
        }

        private static string ToTitle(string key)
        {
            if (string.Equals(key, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase))
                return "Неопределённая форма";

            var n = ParseFormNumber(key);
            if (n.HasValue)
                return $"Форма {n}";

            return key; // Fallback на сам ключ если не удалось распарсить
        }

        private CardViewModelBase CreateCardViewModel(ComponentDraft draft, int number)
        {
            // FORM_4 vs остальные
            if (string.Equals(draft.FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase))
                return new FamilyCardsViewModel(draft.FormCode, "Форма 4", number, draft);

            if (string.Equals(draft.FormCode, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase))
                return new UndefinedCardViewModel(draft.FormCode, ToTitle(draft.FormCode), number, draft);

            return new ComponentCardViewModel(draft.FormCode, draft.FormName, number, draft);
        }

        private static void ReplaceCollection<T>(ObservableCollection<T> collection, List<T> newItems)
        {
            collection.Clear();
            foreach (var item in newItems)
                collection.Add(item);
        }

        private static void EnableCollectionSynchronization(ObservableCollection<CardViewModelBase> collection)
        {
            BindingOperations.EnableCollectionSynchronization(collection, new object());
        }
    }
}
