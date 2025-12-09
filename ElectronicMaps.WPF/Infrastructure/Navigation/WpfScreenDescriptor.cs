using ElectronicMaps.WPF.Infrastructure.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public sealed class WpfScreenDescriptor
    {
        public string Key { get; }
        public string Title { get; }
        public bool IsDefault { get; }
        public string? IconKey { get; }
        public Type ViewModelType { get; }

        public WpfScreenDescriptor(string key, string title, Type viewModelType, bool isDefault = false, string? iconKey = null)
        {
            Key = key ?? throw new ArgumentNullException(nameof(key));
            Title = title ?? throw new ArgumentNullException(nameof(title));
            ViewModelType = viewModelType ?? throw new ArgumentNullException(nameof(viewModelType));
            IsDefault = isDefault;
            IconKey = iconKey;

            if (!typeof(BaseScreenViewModel).IsAssignableFrom(ViewModelType))
            {
                throw new ArgumentException(
                    $"ViewModel type {viewModelType.FullName} must inherit from BaseScreenViewModel",
                    nameof(viewModelType));
            }
        }
    }
}
