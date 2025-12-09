using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Core.Abstractions
{
    /// <summary>
    /// Implemented by screens that need to be notified when they become active.
    /// </summary>
    public interface INavigatedTo
    {
        /// <summary>
        /// Called by the navigation service when the screen becomes the current screen.
        /// </summary>
        /// <param name="parameter">
        ///  Optional parameter passed from the navigation request.
        /// </param>
        /// <param name="cancellationToken">
        /// Token that may be used to observe cancellation of the navigation flow.
        /// </param>
        /// <returns></returns>
        Task OnNavigatedToAsync(object? parameter, CancellationToken cancellationToken = default);
    }
    /// <summary>
    /// Implemented by screens that need to be notified when they are no longer active.
    /// </summary>
    public interface INavigatedFrom
    {
        /// <summary>
        /// Called by the navigation service when the screen stops being the current screen.
        /// </summary>
        /// <param name="cancellationToken">
        /// Token that may be used to observe cancellation of the navigation flow.
        /// </param>
        /// <returns></returns>
        Task OnNavigatedFromAsync(CancellationToken cancellationToken = default);
    }


}
