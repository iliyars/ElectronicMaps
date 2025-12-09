using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Navigation.Core.Abstractions
{
    /// <summary>
    /// Represents a logical "scren" in an application
    /// Typically implemented by a ViewModel or presentation model.
    /// </summary>
    /// /// <remarks>
    /// This interface is intentionally empty and serves as a marker.
    /// It allows the navigation service to work with arbitrary UI frameworks
    /// (WPF, WinUI, MAUI, etc.) without taking dependencies on them.
    /// </remarks>
    public interface IScreen
    {
    }
}
