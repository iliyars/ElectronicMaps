using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace ElectronicMaps.WPF.Features.Workspace
{
    /// <summary>
    /// Interaction logic for WorkspaceView.xaml
    /// </summary>
    public partial class WorkspaceView : UserControl
    {
        private Storyboard? _showStoryboard;
        private Storyboard? _hideSoryboard;

        public WorkspaceView()
        {
            InitializeComponent();
            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.OldValue is WorkspaceViewModel oldVm)
                oldVm.PropertyChanged -= OnViewModelPropertyChanged;

            if (e.NewValue is WorkspaceViewModel newVm)
                newVm.PropertyChanged += OnViewModelPropertyChanged;
        }

        private void OnViewModelPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName != nameof(WorkspaceViewModel.IsDetailsOpen))
                return;

            var vm = (WorkspaceViewModel)sender!;

            if (vm.IsDetailsOpen)
                (FindResource("ShowDetailsPanel") as Storyboard)?.Begin(DetailsPanel);
            else
                (FindResource("HideDetailsPanel") as Storyboard)?.Begin(DetailsPanel);
        }
    }
}
