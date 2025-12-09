using Navigation.Core.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Core.Abstractions
{
    /// <summary>
    /// Provides high-level navigation between logical screens of an application.
    /// </summary>
    /// <remarks>
    /// The navigation service is agnostic of any particular UI framework and
    /// operates solely on <see cref="IScreen"/> instances.
    ///
    /// Typical usage:
    /// <code>
    /// await navigationService.NavigateAsync&lt;MyScreenViewModel&gt;(parameter);
    /// </code>
    /// </remarks>
    public interface INavigationService
    {
        /// <summary>
        /// Gets the current screen instance, or <c>null</c> if navigation
        /// has not yet been performed.
        /// </summary>
        IScreen? CurrentScreen { get; set; }

        /// <summary>
        /// Gets the runtime <see cref="Type"/> of the current screen,
        /// or <c>null</c> if navigation has not yet been performed.
        /// </summary>
        Type? CurrentScreenType { get; }

        // <summary>
        /// Gets the parameter associated with the current navigation,
        /// if any.
        /// </summary>
        object? CurrentParameter { get; }

        /// <summary>
        /// Gets a value indicating whether there is at least one entry
        /// in the back navigation history.
        /// </summary>
        bool CanGoBack { get; }

        /// <summary>
        /// Gets a value indicating whether there is at least one entry
        /// in the forward navigation history.
        /// </summary>
        bool CanGoForward { get; }

        /// <summary>
        /// Navigates to a screen of the specified type.
        /// </summary>
        /// <typeparam name="TScreen">
        /// The screen type to navigate to. Must be registered in the underlying
        /// <see cref="IServiceProvider"/> and implement <see cref="IScreen"/>.
        /// </typeparam>
        /// <param name="parameter">
        /// Optional parameter to pass to the screen. It will be forwarded to
        /// <see cref="INavigatedTo.OnNavigatedToAsync"/> if the screen implements it.
        /// </param>
        /// <param name="addToHistory">
        /// If <c>true</c>, the currently active screen (if any) will be added to
        /// the navigation history so that <see cref="GoBackAsync"/> can return to it.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to observe cancellation of the navigation flow.
        /// </param>
        Task NavigateAsync<TScreen>(object? parameter = null, bool addToHistory = true, 
            CancellationToken cancellationToken = default) where TScreen : class, IScreen;

        /// <summary>
        /// Navigates to a screen of the specified type.
        /// </summary>
        /// <param name="screenType">
        /// The screen type to navigate to. Must be registered in the underlying
        /// <see cref="IServiceProvider"/> and implement <see cref="IScreen"/>.
        /// </param>
        /// <param name="parameter">
        /// Optional parameter to pass to the screen.
        /// </param>
        /// <param name="addToHistory">
        /// If <c>true</c>, the currently active screen (if any) will be added to
        /// the navigation history.
        /// </param>
        /// <param name="cancellationToken">
        /// Token used to observe cancellation of the navigation flow.
        /// </param>
        Task NavigateAsync(
            Type screenType,
            object? parameter = null,
            bool addToHistory = true,
            CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to navigate to the previous entry in the navigation history.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to observe cancellation of the navigation flow.
        /// </param>
        /// <returns>
        /// <c>true</c> if navigation was performed; <c>false</c> if there is no
        /// entry to navigate back to.
        /// </returns>
        Task<bool> GoBackAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Attempts to navigate to the next entry in the navigation history.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token used to observe cancellation of the navigation flow.
        /// </param>
        /// <returns>
        /// <c>true</c> if navigation was performed; <c>false</c> if there is no
        /// entry to navigate forward to.
        /// </returns>
        Task<bool> GoForwardAsync(CancellationToken cancellationToken = default);

        /// <summary>
        /// Occurs when the current screen changes as a result of navigation.
        /// </summary>
        event EventHandler<NavigationChangedEventArgs>? CurrentScreenChanged;
    }
}
