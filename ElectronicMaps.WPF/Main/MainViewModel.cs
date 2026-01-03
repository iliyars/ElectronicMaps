using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.WPF.Features.Welcome;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.ViewModels;
using Navigation.Core.Abstractions;
using Navigation.Core.Models;
using System.Windows;
using System.Windows.Input;


namespace ElectronicMaps.WPF.Main
{
    public class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly IAppCommands _commands;
        
        private bool _disposed = false;

       
        public BaseScreenViewModel? CurrentScreen =>
        _navigation.CurrentScreen as BaseScreenViewModel;

        public bool CanGoBack => _navigation.CanGoBack;
        public bool CanGoForward => _navigation.CanGoForward;

        public IAsyncRelayCommand OpenXmlCommand => _commands.Xml.OpenXml;
        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }
        private string _currentScreenText;
        public string CurrentScreenText
        {
            get => _currentScreenText;
            set
            {
                SetProperty(ref _currentScreenText, value);
            }
        }
        public MainViewModel( INavigationService navigationService, IAppCommands appCommands)
        {
            _navigation = navigationService;
            _commands = appCommands;
            //SwitchTheme(true);
            _navigation.CurrentScreenChanged += OnCurrentScreenChanged;
        }
     
       
        private void OnCurrentScreenChanged(object? sender, NavigationChangedEventArgs e)
        {
            // если используешь AsyncRelayCommand из MVVM Toolkit —
            // можно дернуть NotifyCanExecuteChanged для Back/Forward
            if (GoBackCommand is IRelayCommand backRelay)
                backRelay.NotifyCanExecuteChanged();

            if (GoForwardCommand is IRelayCommand forwardRelay)
                forwardRelay.NotifyCanExecuteChanged();
            OnPropertyChanged(nameof(CurrentScreen));
            CurrentScreenText = CurrentScreen.GetType().ToString();
            OnPropertyChanged(nameof(CanGoBack));
            OnPropertyChanged(nameof(CanGoForward));
            OnPropertyChanged(nameof(CurrentScreenText));
        }

       

        private bool _isDetailsOpen;
        public bool IsDetailsOpen
        {
            get => _isDetailsOpen;
            set => SetProperty(ref _isDetailsOpen, value);
        }



        public void SwitchTheme(bool isDark)
        {
            var dictionary = isDark
                ? new ResourceDictionary { Source = new Uri("pack://application:,,,/ElectronicMaps.WPF;component/Themes/DarkTheme.xaml") }
                : new ResourceDictionary { Source = new Uri("pack://application:,,,/ElectronicMaps.WPF;component/Themes/LightTheme.xaml") };

            // Применяем новую тему
            System.Windows.Application.Current.Resources.MergedDictionaries.Clear();
            System.Windows.Application.Current.Resources.MergedDictionaries.Add(dictionary);
        }


        public async Task InitializeAsync()
        {
            if(CurrentScreen is null)
            {
                await _navigation.NavigateAsync<WelcomeViewModel>();
            }
        }
    }
}
