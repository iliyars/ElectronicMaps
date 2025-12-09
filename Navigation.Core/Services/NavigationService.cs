using Navigation.Core.Abstractions;
using Navigation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Core.Services
{
    /// <summary>
    /// Default implementation of <see cref="INavigationService"/> that uses an
    /// <see cref="IServiceProvider"/> to resolve screen instances and maintains
    /// in-memory back/forward navigation history.
    /// </summary>
    /// <remarks>
    /// This implementation is not thread-safe. It is intended to be used from
    /// a single UI thread typical for desktop applications.
    /// </remarks>
    public class NavigationService : INavigationService, IDisposable
    {
        private readonly IServiceProvider _serviceProvider;

        private readonly Stack<NavigationEntry> _backStack = new();
        private readonly Stack<NavigationEntry> _forwardStack = new();

        private bool _disposed;

        /// <inheritdoc/>
        public IScreen? CurrentScreen { get;  set; }
        /// <inheritdoc/>
        public Type? CurrentScreenType => CurrentScreen?.GetType();
        /// <inheritdoc/>
        public object? CurrentParameter { get; private set; }
        /// <inheritdoc/>
        public bool CanGoBack => _backStack.Count > 0;
        /// <inheritdoc/>
        public bool CanGoForward => _forwardStack.Count > 0;

        /// <inheritdoc />
        public event EventHandler<NavigationChangedEventArgs>? CurrentScreenChanged;
        /// <summary>
        /// Represents a single navigation history entry.
        /// </summary>
        private sealed record NavigationEntry(IScreen Screen, object? Parameter);

        public NavigationService(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider ?? throw new ArgumentNullException(nameof(serviceProvider));
        }

        /// <inheritdoc />
        public async Task<bool> GoBackAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!CanGoBack)
                return false;

            // Move current into forward stack (if any)
            var oldScreen = CurrentScreen;
            var oldParameter = CurrentParameter;

            if (oldScreen is not null)
            {
                _forwardStack.Push(new NavigationEntry(oldScreen, oldParameter));
            }

            var entry = _backStack.Pop();

            // Notify old screen
            if (oldScreen is INavigatedFrom from)
            {
                await from.OnNavigatedFromAsync(cancellationToken).ConfigureAwait(false);
            }

            // Activate new screen
            CurrentScreen = entry.Screen;
            CurrentParameter = entry.Parameter;

            if (entry.Screen is INavigatedTo to)
            {
                await to.OnNavigatedToAsync(entry.Parameter, cancellationToken).ConfigureAwait(false);
            }

            RaiseCurrentScreenChanged(oldScreen, entry.Parameter);
            return true;
        }
        /// <inheritdoc />
        public async Task<bool> GoForwardAsync(CancellationToken cancellationToken = default)
        {
            ThrowIfDisposed();

            if (!CanGoForward)
                return false;

            var oldScreen = CurrentScreen;
            var oldParameter = CurrentParameter;

            if (oldScreen is not null)
            {
                _backStack.Push(new NavigationEntry(oldScreen, oldParameter));
            }

            var entry = _forwardStack.Pop();

            if (oldScreen is INavigatedFrom from)
            {
                await from.OnNavigatedFromAsync(cancellationToken).ConfigureAwait(false);
            }

            CurrentScreen = entry.Screen;
            CurrentParameter = entry.Parameter;

            if (entry.Screen is INavigatedTo to)
            {
                await to.OnNavigatedToAsync(entry.Parameter, cancellationToken).ConfigureAwait(false);
            }

            RaiseCurrentScreenChanged(oldScreen, entry.Parameter);
            return true;
        }
        /// <inheritdoc />
        public Task NavigateAsync(Type screenType, object? parameter = null, bool addToHistory = true, CancellationToken cancellationToken = default)
        {
            if (screenType is null)
                throw new ArgumentNullException(nameof(screenType));

            if (!typeof(IScreen).IsAssignableFrom(screenType))
            {
                throw new ArgumentException(
                    $"The specified type '{screenType.FullName}' does not implement {nameof(IScreen)}.",
                    nameof(screenType));
            }

            return NavigateInternalAsync(screenType, parameter, addToHistory, cancellationToken);
        }

        public Task NavigateAsync<TScreen>(object? parameter, bool addToHistory, CancellationToken cancellationToken) where TScreen : class, IScreen
        {
            return NavigateInternalAsync(typeof(TScreen), parameter, addToHistory, cancellationToken);
        }
        /// <summary>
        /// Releases resources held by the navigation service.
        /// </summary>
        /// <remarks>
        /// If screen instances implement <see cref="IDisposable"/>, they will be
        /// disposed when the navigation service is disposed.
        /// </remarks>
        public void Dispose()
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            DisposeScreen(CurrentScreen);

            foreach (var entry in _backStack)
            {
                DisposeScreen(entry.Screen);
            }

            foreach (var entry in _forwardStack)
            {
                DisposeScreen(entry.Screen);
            }

            _backStack.Clear();
            _forwardStack.Clear();
        }
        

        private async Task NavigateInternalAsync(
            Type screenType,
            object? parameter,
            bool addToHistory,
            CancellationToken cancellationToken)
        {
            ThrowIfDisposed();

            System.Diagnostics.Debug.WriteLine(
        $"[NavigateInternalAsync] Thread: {Environment.CurrentManagedThreadId}");


            var oldScreen = CurrentScreen;
            var oldParameter = CurrentParameter;

            if (addToHistory && oldScreen is not null)
            {
                _backStack.Push(new NavigationEntry(oldScreen, oldParameter));
                _forwardStack.Clear();
            }

            if (oldScreen is INavigatedFrom from)
            {
                await from.OnNavigatedFromAsync(cancellationToken);
            }

            // Resolve new screen via DI
            var resolved = _serviceProvider.GetService(screenType);
            if (resolved is null)
            {
                throw new InvalidOperationException(
                    $"Unable to resolve screen of type '{screenType.FullName}' from the service provider.");
            }

            if (resolved is not IScreen newScreen)
            {
                throw new InvalidOperationException(
                    $"Resolved service of type '{screenType.FullName}' does not implement {nameof(IScreen)}.");
            }

            CurrentScreen = newScreen;
            CurrentParameter = parameter;

            if (newScreen is INavigatedTo to)
            {
                await to.OnNavigatedToAsync(parameter, cancellationToken);
            }

            RaiseCurrentScreenChanged(oldScreen, parameter);
        }

        private void RaiseCurrentScreenChanged(IScreen? oldScreen, object? parameter)
        {
            var args = new NavigationChangedEventArgs(oldScreen, CurrentScreen, parameter);
            CurrentScreenChanged?.Invoke(this, args);
        }

        private void ThrowIfDisposed()
        {
            if (_disposed)
            {
                throw new ObjectDisposedException(nameof(NavigationService));
            }
        }

        private static void DisposeScreen(IScreen? screen)
        {
            if (screen is IDisposable disposable)
            {
                disposable.Dispose();
            }
        }

        
    }
}
