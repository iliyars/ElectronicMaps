using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    internal sealed class NavigationEntry
    {
        public string ScreenKey { get; }
        public object? Parameter { get; }

        public NavigationEntry(string screenKey, object? parameter)
        {
            ScreenKey = screenKey;
            Parameter = parameter;
        }

    }
}
