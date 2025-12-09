using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using ElectronicMaps.Application.DTO;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.Screens;
using ElectronicMaps.WPF.Services.Dialogs;
using MaterialDesignColors;
using MaterialDesignThemes.Wpf;
using Navigation.Core.Abstractions;
using Navigation.Core.Models;
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
    public class MainViewModel : ObservableObject
    {
        private readonly INavigationService _navigation;
        private readonly IAppCommands _commands;
        
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
            _navigation.CurrentScreenChanged += OnCurrentScreenChanged;
            System.Diagnostics.Debug.WriteLine(
        $"MainViewModel created: {GetHashCode()}");
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

            System.Diagnostics.Debug.WriteLine(
        $"[NavigateInternalAsync] Thread: {Environment.CurrentManagedThreadId}");

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


        

       

        public async Task InitializeAsync()
        {
            if(CurrentScreen is null)
            {
                await _navigation.NavigateAsync<WelcomeViewModel>();
            }
        }
    }
}
