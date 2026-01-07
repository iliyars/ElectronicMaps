using ElectronicMaps.WPF.Features.Workspace.Components.ViewModels;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace ElectronicMaps.WPF.Features.Workspace.Components
{
    /// <summary>
    /// Interaction logic for CreateComponentDialog.xaml
    /// </summary>
    public partial class CreateComponentDialog : Window
    {
        public CreateComponentDialog()
        {
            InitializeComponent();

            DataContextChanged += OnDataContextChanged;
        }

        private void OnDataContextChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue is CreateComponentViewModel viewModel)
            {
                // Установить action для закрытия окна
                viewModel.CloseAction = () =>
                {
                    DialogResult = viewModel.DialogResult;
                    Close();
                };
            }
        }
    }
}
