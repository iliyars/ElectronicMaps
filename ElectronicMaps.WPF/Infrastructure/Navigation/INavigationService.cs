using ElectronicMaps.Application.Common.Navigation;
using ElectronicMaps.WPF.Infrastructure.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public interface INavigationService
    {

        /// <summary>
        /// Текущий экран (ViewModel), который должен отображаться в Shell.
        /// Может быть null на самом старте.
        /// </summary>
        BaseScreenViewModel? CurrentScreen { get; }

        /// <summary>
        /// Дескриптор текущего экрана (ключ, заголовок и т.д.).
        /// </summary>
        WpfScreenDescriptor? CurrentDescriptor { get; }

        /// <summary>
        /// Переход на экран по ключу.
        /// </summary>
        /// <param name="screenKey">Ключ экрана (см. ScreenKeys).</param>
        /// <param name="parameter">Опциональный параметр для инициализации экрана.</param>
        /// <param name="addToHistory">Добавлять ли экран в историю навигации.</param>
        Task NavigateAsync(string screenKey, object? parameter = null, bool addToHistory = true);

        /// <summary>
        /// Можно ли в принципе попытаться перейти на этот экран.
        /// </summary>
        bool CanNavigate(string screenKey);

        /// <summary>
        /// Есть ли куда вернуться назад.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Есть ли куда перейти вперёд (после Back).
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Переход к предыдущему экрану в истории.
        /// Возвращает true, если переход состоялся.
        /// </summary>
        Task<bool> GoBackAsync();

        /// <summary>
        /// Переход к следующему экрану в истории.
        /// Возвращает true, если переход состоялся.
        /// </summary>
        Task<bool> GoForwardAsync();

        /// <summary>
        /// Событие вызывается при смене текущего экрана.
        /// </summary>
        event EventHandler<NavigationChangedEventArgs> CurrentScreenChanged;

    }

    /// <summary>
    /// Аргументы события смены экрана.
    /// </summary>
    public sealed class NavigationChangedEventArgs : EventArgs
    {
        public BaseScreenViewModel? OldScreen { get; }
        public BaseScreenViewModel? NewScreen { get; }
        public WpfScreenDescriptor? OldDescriptor { get; }
        public WpfScreenDescriptor? NewDescriptor { get; }
        public object? Parameter { get; }

        public NavigationChangedEventArgs(
            BaseScreenViewModel? oldScreen,
            BaseScreenViewModel? newScreen,
            WpfScreenDescriptor? oldDescriptor,
            WpfScreenDescriptor? newDescriptor,
            object? parameter)
        {
            OldScreen = oldScreen;
            NewScreen = newScreen;
            OldDescriptor = oldDescriptor;
            NewDescriptor = newDescriptor;
            Parameter = parameter;
        }
    }

}
