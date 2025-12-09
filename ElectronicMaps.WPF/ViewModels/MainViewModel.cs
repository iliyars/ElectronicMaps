using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicMaps.Application.Common.Navigation;
using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.Navigation;
using ElectronicMaps.WPF.Infrastructure.Screens;
using ElectronicMaps.WPF.Services.Dialogs;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace ElectronicMaps.WPF.ViewModels
{
    public class MainViewModel : ViewModelBase
    {
        private readonly IAppCommands _commands;
        private INavigationService _navigationService;
        private BaseScreenViewModel? _currentScreen;
        public BaseScreenViewModel? CurrentScreen
        {
            get => _currentScreen;
            private set => SetProperty(ref _currentScreen, value);
        }
        private bool _disposed = false;

        private bool _isDarkTheme;
        public bool IsDarkTheme
        {
            get => _isDarkTheme;
            set
            {
                if (SetProperty(ref _isDarkTheme, value))
                {
                    ApplyTheme(value);
                }
            }
        }

        public IAsyncRelayCommand OpenXmlCommand => _commands.Xml.OpenXml;
        public ICommand GoBackCommand { get; }
        public ICommand GoForwardCommand { get; }

        public MainViewModel( INavigationService navigationService, IAppCommands appCommands)
        {
            _navigationService = navigationService;
            _commands = appCommands;
            _navigationService.CurrentScreenChanged += OnCurrentScreenChanged;

            CurrentScreen = _navigationService.CurrentScreen;

            InitThemeFromResources();
        }
        private void InitThemeFromResources()
        {
            var app = System.Windows.Application.Current;

            var theme = app.Resources
                           .MergedDictionaries
                           .OfType<BundledTheme>()
                           .FirstOrDefault();

            if (theme != null)
            {
                IsDarkTheme = theme.BaseTheme == BaseTheme.Dark;
            }
        }

        private void ApplyTheme(bool isDark)
        {
            var app = System.Windows.Application.Current;
            var resources = app.Resources;

            // 1. Переключаем MaterialDesign BundledTheme
            var bundledTheme = resources
                .MergedDictionaries
                .OfType<BundledTheme>()
                .FirstOrDefault();

            if (bundledTheme != null)
            {
                bundledTheme.BaseTheme = isDark ? BaseTheme.Dark : BaseTheme.Light;

                // Акцентные цвета (можно оставить одинаковыми)
                bundledTheme.PrimaryColor = PrimaryColor.Teal;
                bundledTheme.SecondaryColor = isDark
                    ? SecondaryColor.Lime
                    : SecondaryColor.Amber;
            }

            // 2. Подменяем наши брендовые кисти под Light/Dark

            Color GetColor(string key) => (Color)resources[key];

            resources["AppBackgroundBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppBackgroundDarkColor") : GetColor("AppBackgroundLightColor"));

            resources["AppSurfaceBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppSurfaceDarkColor") : GetColor("AppSurfaceLightColor"));

            resources["AppToolbarBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppToolbarDarkColor") : GetColor("AppToolbarLightColor"));

            resources["AppBorderBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppBorderDarkColor") : GetColor("AppBorderLightColor"));

            resources["AppHeaderTextBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppHeaderTextDarkColor") : GetColor("AppHeaderTextLightColor"));

            resources["AppBodyTextBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppBodyTextDarkColor") : GetColor("AppBodyTextLightColor"));

            resources["AppMutedTextBrush"] = new SolidColorBrush(
                isDark ? GetColor("AppMutedTextDarkColor") : GetColor("AppMutedTextLightColor"));

        }

        private void OnCurrentScreenChanged(object? sender, NavigationChangedEventArgs e)
        {
            CurrentScreen = e.NewScreen;

            // если используешь AsyncRelayCommand из MVVM Toolkit —
            // можно дернуть NotifyCanExecuteChanged для Back/Forward
            if (GoBackCommand is IRelayCommand backRelay)
                backRelay.NotifyCanExecuteChanged();

            if (GoForwardCommand is IRelayCommand forwardRelay)
                forwardRelay.NotifyCanExecuteChanged();
        }

        public ObservableCollection<string> NavigationItems { get; } = new()
        {
            "Project", "Components", "Forms", "Merge", "Settings"
        };
        
        private string _selectedNavigationItem;
        public string SelectedNavigationItem
        {
            get => _selectedNavigationItem;
            set
            {
                if (SetProperty(ref _selectedNavigationItem, value))
                    return;
                //OnNavigateChanged(value);
            }
        }

        private bool _isDetailsOpen;
        public bool IsDetailsOpen
        {
            get => _isDetailsOpen;
            set => SetProperty(ref _isDetailsOpen, value);
        }


        

       

        public async Task InitializeAsync()
        {
            if(CurrentScreen is null)
            {
                await _navigationService.NavigateAsync(ScreenKeys.Welcome);
            }
        }
    }
}
