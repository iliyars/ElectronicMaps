using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Application.WorkspaceProject.Models;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.FormCards;
using ElectronicMaps.WPF.Features.Workspace.ViewModels.GridRows;
using ElectronicMaps.WPF.Infrastructure.Screens;
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
    public partial class WorkspaceViewModel : BaseScreenViewModel, INavigatedTo, INavigatedFrom
    {
        private readonly IComponentStore _componentStore;
        private bool _subscribed;

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
            ItemCards.Clear();

            var key = selectedViewKey ?? "ALL";
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
            if (draft != null) return;

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
            if (existing != null) return;

            ItemCards.Remove(existing);

            for(int i = 0; i < ItemCards.Count; i++)
            {
                ItemCards[i].Number = i + 1;
            }

            // если удалили открытую карточку — закрываем детали
            if (OpenDetailsDraftId == id)
                OpenDetailsDraftId = null;
        }


        



        private CardViewModelBase CreateCardViewModel(ComponentDraft draft, int number)
        {
            // FORM_4 vs остальные
            if (string.Equals(draft.FormCode, "FORM_4", StringComparison.OrdinalIgnoreCase))
                return new FamilyCardsViewModel(draft.FormCode, "Форма 4", number, draft);

            return new ComponentCardViewModel(draft.FormCode, draft.FormName, number, draft);
        }






       

        //public void Dispose()
        //{
        //    if(_subscribed)
        //    {
        //        _componentStore.Changed -= OnStoreChanged;
        //        _subscribed = false;
        //    }    
        //}

       
    }
}
