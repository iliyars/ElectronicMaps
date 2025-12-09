using ElectronicMaps.Application.Common.Navigation;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public class ScreenRegistry : IScreenRegistry
    {
        private readonly Dictionary<string, WpfScreenDescriptor> _map =
           new(StringComparer.OrdinalIgnoreCase);

        public void Register(WpfScreenDescriptor descriptor)
        {
            if (descriptor is null)
                throw new ArgumentNullException(nameof(descriptor));

            _map[descriptor.Key] = descriptor;
        }

        public WpfScreenDescriptor? Find(string key)
        {
            if (key is null)
                throw new ArgumentNullException(nameof(key));

            return _map.TryGetValue(key, out var descriptor) ? descriptor : null;
        }

        public IReadOnlyCollection<WpfScreenDescriptor> GetAll() =>
            _map.Values.ToList().AsReadOnly();
    }
}
