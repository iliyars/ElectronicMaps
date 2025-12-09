using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.WPF.ViewModels.Components;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels
{
    public class ComponentsViewModel : ObservableObject
    {

        public ObservableCollection<ComponentRowViewModel> Items { get; } = new ObservableCollection<ComponentRowViewModel>();


        // view для фильтрации и сортировки
        public ICollectionView ItemsView { get; }

        public ObservableCollection<ComponentRowViewModel> SelectedItems { get; } = new ObservableCollection<ComponentRowViewModel>();



    }
}
