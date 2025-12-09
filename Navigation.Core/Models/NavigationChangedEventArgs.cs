using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Core.Models
{
    /// <summary>
    /// Provides data for the <see cref="INavigationService.CurrentScreenChanged"/> event.
    /// </summary>
    public class NavigationChangedEventArgs : EventArgs
    {
        /// <summary>
        /// Gets the previously active screen instance, or <c>null</c> if there was none.
        /// </summary>
        public IScreen? OldScreen { get; }

        /// <summary>
        /// Gets the newly active screen instance, or <c>null</c> in rare cases
        /// when navigation resulted in no active screen.
        /// </summary>
        public IScreen? NewScreen { get; }

        // <summary>
        /// Gets the parameter associated with the navigation that produced
        /// <see cref="NewScreen"/>, if any.
        /// </summary>
        public object? Parameter { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NavigationChangedEventArgs"/> class.
        /// </summary>
        /// <param name="oldScreen">The previously active screen.</param>
        /// <param name="newScreen">The newly active screen.</param>
        /// <param name="parameter">The navigation parameter, if any.</param>
        public NavigationChangedEventArgs(
            IScreen? oldScreen,
            IScreen? newScreen,
            object? parameter)
        {
            OldScreen = oldScreen;
            NewScreen = newScreen;
            Parameter = parameter;
        }
    }
}
