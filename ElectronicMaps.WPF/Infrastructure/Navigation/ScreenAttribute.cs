using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    [AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
    public sealed class ScreenAttribute : Attribute
    {
        public string Key { get; }
        public string Title { get; }
        public bool IsDefault { get; }
        public string? IconKey { get; }

        public ScreenAttribute(string key, string title, bool isDefault = false, string? iconKey = null)
        {
            Key = key;
            Title = title;
            IsDefault = isDefault;
            IconKey = iconKey;
        }
    }
}
