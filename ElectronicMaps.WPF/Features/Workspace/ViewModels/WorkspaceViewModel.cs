using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Microsoft.EntityFrameworkCore.Scaffolding.Metadata;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;

namespace ElectronicMaps.WPF.Features.Workspace.ViewModels
{

    public record ViewOptions(string Key, string Title);

    public partial class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo, INavigatedFrom
    {
        private readonly IComponentStore _componentStore;
        private bool _subscribed;

        public ObservableCollection<ViewOptions> ViewOptions { get; } = new();
        public ObservableCollection<CardViewModelBase> ItemCards { get; } = new();

        /// <summary>
        /// Все компоненты из xml.
        /// </summary>
        public ObservableCollection<ImportedRowViewModel> ImportedComponents { get; } = new();


        [ObservableProperty]
        private bool isDetailsOpen;
        
        [ObservableProperty]
        private string? selectedViewKey;

        [ObservableProperty]
        private Guid? openDetailsDraftId;

        partial void OnSelectedViewKeyChanged(string? value)
        {
            RebuildCards();
        }

        public IRelayCommand<Guid> ToggleDetailsCommand { get; }
        public WorkspaceViewModel(IComponentStore componentStore)
        {
            _componentStore = componentStore;

            ToggleDetailsCommand = new RelayCommand<Guid>(ToggleDetails);
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
            OpenDetailsDraftId = OpenDetailsDraftId == draftId ? null : draftId;
            SyncDetailsState();
        }

        private void OnStoreChanged(object? sender, StoreChangedEventArgs e)
        {
            // ВАЖНО: если store меняется из background (БД/файлы), обновляем UI через Dispatcher
            if(!System.Windows.Application.Current.Dispatcher.CheckAccess())
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
            foreach(var card in ItemCards)
            {
                card.IsDetailsOpen = OpenDetailsDraftId.HasValue && card.Item.Id == OpenDetailsDraftId.Value;
            }
        }

        private void RebuildCards()
        {
            RebuildViewOptions();

            ItemCards.Clear();

            var key = SelectedViewKey ?? WorkspaceViewKeys.UndefinedForm;
            var drafts = _componentStore.GetWorkingForView(key);

            var number = 1;
            foreach (var draft in drafts)
            {
                ItemCards.Add(CreateCardViewModel(draft, number++));
            }
        }

        private void UpsertCard(Guid id)
        {
            var draft = _componentStore.TryGetWorking(id);
            if (draft == null) return;

            var existing = ItemCards.FirstOrDefault(c => c.Item.Id == id);
            if (existing is not null)
            {
                existing.ReplaceItem(draft);
                return;
            }

            // Если карточки ещё нет — добавляем (позиционирование можно улучшить позже)
            var number = ItemCards.Count + 1;
            ItemCards.Add(CreateCardViewModel(draft, number));
        }

        private void RemoveCard(Guid id)
        {
            var existing = ItemCards.FirstOrDefault(c => c.Item.Id == id);
            if (existing == null) return;

            ItemCards.Remove(existing);

            for(int i = 0; i < ItemCards.Count; i++)
            {
                ItemCards[i].Number = i + 1;
            }

            // если удалили открытую карточку — закрываем детали
            if (OpenDetailsDraftId == id)
                OpenDetailsDraftId = null;
        }

        private void RebuildViewOptions()
        {
            ViewOptions.Clear();

            //Ключи форм из store
            var keys = _componentStore.GetViewKeys();

            // сортировка: undefind
            var ordered = keys.OrderBy(k => string.Equals(k, WorkspaceViewKeys.UndefinedForm, StringComparison.OrdinalIgnoreCase)? 0 : 1)
                .ThenBy(keys => ParseFormNumber(keys) ?? int.MaxValue)
                .ThenBy(keys => keys, StringComparer.OrdinalIgnoreCase);

            foreach(var key in ordered)
                ViewOptions.Add(new ViewOptions(key, ToTitle(key)));

            // если ничего не выбрано - выставим default
            if(string.IsNullOrWhiteSpace(SelectedViewKey))
                SelectedViewKey = ViewOptions.FirstOrDefault()?.Key;
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
            return $"Форма {n}";
        }


        private CardViewModelBase CreateCardViewModel(ComponentDraft draft, int number)
        {
            // FORM_4 vs остальные
            if (string.Equals(draft.FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase))
                return new FamilyCardsViewModel(draft.FormCode, "Форма 4", number, draft);

            return new ComponentCardViewModel(draft.FormCode, draft.FormName, number, draft);
        }
    }
}
