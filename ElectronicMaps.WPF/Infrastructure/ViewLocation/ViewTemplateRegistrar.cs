using ElectronicMaps.WPF.Infrastructure.ViewModels;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ElectronicMaps.WPF.Infrastructure.ViewLocation
{
    public static class ViewTemplateRegistrar
    {
        /// <summary>
        /// Регистрирует DataTemplate-ы View по соглашению:
        /// WelcomeViewModel -> WelcomeView (тот же namespace).
        /// </summary>
        public static void RegisterViewTemplates(Assembly assembly)
        {
            if (assembly == null)
                throw new ArgumentNullException(nameof(assembly));

            // Все ViewModel экранов
            var viewModelTypes = assembly
                .GetTypes()
                .Where(t =>
                    !t.IsAbstract &&
                    typeof(BaseScreenViewModel).IsAssignableFrom(t) &&
                    t.Name.EndsWith("ViewModel", StringComparison.Ordinal));

            foreach (var vmType in viewModelTypes)
            {
                // Имя View: WelcomeViewModel -> WelcomeView
                var viewTypeName = vmType.Name.Replace("Model", string.Empty); // ViewModel -> View

                // Ищем View в той же сборке и namespace
                var viewType = assembly.GetType(vmType.Namespace + "." + viewTypeName);

                if (viewType == null)
                {
                    // View не нашли — просто пропускаем, не падаем
                    continue;
                }

                if (!typeof(UserControl).IsAssignableFrom(viewType))
                {
                    // На всякий случай убеждаемся, что это UserControl
                    continue;
                }

                // Создаём DataTemplate: DataType = ViewModel, VisualTree = View
                var template = new DataTemplate
                {
                    DataType = vmType
                };

                var factory = new FrameworkElementFactory(viewType);
                template.VisualTree = factory;

                // Кладём в глобальные ресурсы приложения
                System.Windows.Application.Current.Resources.Add(template.DataTemplateKey, template);
            }
        }
    }
}
