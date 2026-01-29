using ElectronicMaps.Application.Features.Workspace.Models;
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

    private readonly CreateComponentViewModel _viewModel;
    public CreateComponentDialog(CreateComponentViewModel viewModel)
    {
      InitializeComponent();

      _viewModel = viewModel;
      DataContext = _viewModel;


      _viewModel.CloseAction = () =>
      {
        this.DialogResult = _viewModel.DialogResult;
      };
    }

    public bool? ShowDialogWithDraft(ComponentDraft draft)
    {
      Loaded += async (s, e) =>
      {
        await _viewModel.InitializeAsync(draft);
      };

      return ShowDialog();
    }
  }
}
