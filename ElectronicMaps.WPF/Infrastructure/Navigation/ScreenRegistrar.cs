using ElectronicMaps.WPF.Infrastructure.Screens;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Infrastructure.Navigation
{
    public static class ScreenRegistrar
    {
        public static void RegisterScreensFromAssemblies(
            IScreenRegistry registry,
            params Assembly[] assemblies)
        {
            if (registry is null)
                throw new ArgumentNullException(nameof(registry));

            if (assemblies is null || assemblies.Length == 0)
            {
                assemblies = new[] { Assembly.GetExecutingAssembly() };
            }

            foreach (var assembly in assemblies)
            {
                var screenVmTypes = assembly
                    .GetTypes()
                    .Where(t =>
                        !t.IsAbstract &&
                        typeof(BaseScreenViewModel).IsAssignableFrom(t) &&
                        t.GetCustomAttribute<ScreenAttribute>() != null);

                foreach (var vmType in screenVmTypes)
                {
                    var attr = vmType.GetCustomAttribute<ScreenAttribute>()!;
                    var descriptor = new WpfScreenDescriptor(
                        key: attr.Key,
                        title: attr.Title,
                        viewModelType: vmType,
                        isDefault: attr.IsDefault,
                        iconKey: attr.IconKey);

                    registry.Register(descriptor);
                }
            }
        }



    }
}
