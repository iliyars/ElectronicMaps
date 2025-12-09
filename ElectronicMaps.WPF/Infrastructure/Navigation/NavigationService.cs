using ElectronicMaps.Application.Common.Navigation;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public class NavigationService : INavigationService
    {
        private readonly IScreenRegistry _screenRegistry;
        private readonly IServiceProvider _serviceProvider;

        private readonly Stack<NavigationEntry> _backStack = new();
        private readonly Stack<NavigationEntry> _forwardStack = new();

        private BaseScreenViewModel? _currentScreen;
        private WpfScreenDescriptor? _currentDescriptor;

        public BaseScreenViewModel? CurrentScreen => _currentScreen;
        public WpfScreenDescriptor? CurrentDescriptor => _currentDescriptor;

        public bool CanGoBack => _backStack.Count > 0;
        public bool CanGoForward => _forwardStack.Count > 0;


        public event EventHandler<NavigationChangedEventArgs>? CurrentScreenChanged;

        public NavigationService(IScreenRegistry screenRegistry, IServiceProvider serviceProvider)
        {
            _screenRegistry = screenRegistry ?? throw new ArgumentNullException(nameof(screenRegistry));
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        public bool CanNavigate(string screenKey)
        {
            if (screenKey is null)
                throw new ArgumentNullException(nameof(screenKey));

            return _screenRegistry.Find(screenKey) is not null;
        }

        public async Task NavigateAsync(string screenKey, object? parameter = null, bool addToHistory = true)
        {
            if (!CanNavigate(screenKey))
                throw new InvalidOperationException($"Screen with key '{screenKey}' is not registered.");

            var descriptor = _screenRegistry.Find(screenKey)!;

            var oldScreen = _currentScreen;
            var oldDescriptor = _currentDescriptor;

            // если нужно, добавляем текущий экран в back-стек
            if (addToHistory && oldDescriptor != null)
            {
                _backStack.Push(new NavigationEntry(oldDescriptor.Key, parameter: null)); // параметр истории по желанию
                _forwardStack.Clear(); // после прямой навигации вперёд историю вперёд обычно чистят
            }

            // вызываем OnLeaveAsync у старого экрана
            if (oldScreen is not null)
            {
                await oldScreen.OnLeaveAsync();
            }

            // создаём новый ViewModel через DI
            var vm = (BaseScreenViewModel)_serviceProvider.GetRequiredService(descriptor.ViewModelType);

            // обновляем текущие значения
            _currentScreen = vm;
            _currentDescriptor = descriptor;

            // вызываем OnEnterAsync у нового экрана
            await vm.OnEnterAsync(parameter);

            // уведомляем подписчиков (Shell и т.п.)
            CurrentScreenChanged?.Invoke(this, new NavigationChangedEventArgs(
                oldScreen,
                _currentScreen,
                oldDescriptor,
                _currentDescriptor,
                parameter));
        }

        public async Task<bool> GoBackAsync()
        {
            if (!CanGoBack)
                return false;

            var entry = _backStack.Pop();

            // текущий экран уйдёт в forward-стек
            if (_currentDescriptor != null)
            {
                _forwardStack.Push(new NavigationEntry(_currentDescriptor.Key, parameter: null));
            }

            await NavigateAsync(entry.ScreenKey, entry.Parameter, addToHistory: false);
            return true;
        }

        public async Task<bool> GoForwardAsync()
        {
            if (!CanGoForward)
                return false;

            var entry = _forwardStack.Pop();

            // текущий экран уйдёт обратно в back-стек
            if (_currentDescriptor != null)
            {
                _backStack.Push(new NavigationEntry(_currentDescriptor.Key, parameter: null));
            }

            await NavigateAsync(entry.ScreenKey, entry.Parameter, addToHistory: false);
            return true;
        }
    }
}
