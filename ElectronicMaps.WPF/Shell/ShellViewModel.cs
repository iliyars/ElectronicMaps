using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.WPF.Infrastructure.Screens;
using ElectronicMaps.WPF.ViewModels;
using Navigation.Core.Abstractions;
using Navigation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElectronicMaps.WPF.Shell
{
    public class ShellViewModel : ViewModelBase
    {
        private readonly INavigationService _navigationService;
        // опционально: IUndoRedoService _undoRedoService;
        // опционально: IAppCapabilities _capabilities;

        private BaseScreenViewModel? _currentScreen;
        private string _windowTitle = "ElectronicMaps";

        public BaseScreenViewModel? CurrentScreen
        {
            get => _currentScreen;
            private set
            {
                if (_currentScreen != value)
                {
                    _currentScreen = value;
                    OnPropertyChanged();
                }
            }
        }

        /// <summary>
        /// Заголовок окна. Можно собирать из названия приложения + текущего экрана.
        /// </summary>
        public string WindowTitle
        {
            get => _windowTitle;
            private set
            {
                if (_windowTitle != value)
                {
                    _windowTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NavigateToComponentsCommand { get; }
        public ICommand NavigateToFormsCommand { get; }

        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }

        // public ICommand UndoCommand { get; }
        // public ICommand RedoCommand { get; }

        public ShellViewModel(INavigationService navigationService /*, IUndoRedoService undoRedoService, IAppCapabilities capabilities*/)
        {
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            //_undoRedoService = undoRedoService;
            //_capabilities = capabilities;

           
            GoBackCommand = new AsyncRelayCommand(
                () => _navigationService.GoBackAsync(),
                () => _navigationService.CanGoBack);

            GoForwardCommand = new AsyncRelayCommand(
                () => _navigationService.GoForwardAsync(),
                () => _navigationService.CanGoForward);

            // Команды Undo/Redo можно повесить на IUndoRedoService
            // UndoCommand = new AsyncRelayCommand(_undoRedoService.UndoAsync, _undoRedoService.CanUndo);
            // RedoCommand = new AsyncRelayCommand(_undoRedoService.RedoAsync, _undoRedoService.CanRedo);
        }

        private void OnCurrentScreenChanged(object? sender, NavigationChangedEventArgs e)
        {



        }

       
        /// <summary>
        /// Вызывается при старте приложения, чтобы открыть экран по умолчанию.
        /// Можно звать из App.xaml.cs.
        /// </summary>
        public async Task InitializeAsync()
        {
            // Например, перейти на экран компонентов, если ничего не выбрано.
            if (CurrentScreen is null)
            {
            }
        }
    }
}
