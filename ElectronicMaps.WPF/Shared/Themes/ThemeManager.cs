using System;
using System.Collections.Generic;
using System.Text;
using System.Windows;

namespace ElectronicMaps.WPF.Shared.Themes
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        private const string LightThemeUri = "Themes/LightTheme.xaml";
        private const string DarkThemeUri = "Themes/DarkTheme.xaml";

        public static ThemeType CurrentTheme { get; set; }

        public static void SetTheme(ThemeType theme)
        {
            CurrentTheme = theme;

            string themeUri = theme == ThemeType.Light ? LightThemeUri : DarkThemeUri;

            var newTheme = new ResourceDictionary { Source = new Uri(themeUri, UriKind.Relative) };

            var existingTheme = System.Windows.Application.Current.Resources.MergedDictionaries
                .FirstOrDefault(d => d.Source?.OriginalString.Contains("Theme.xaml") == true);

            if (existingTheme != null)
            {
                System.Windows.Application.Current.Resources.MergedDictionaries.Remove(existingTheme);
            }

            System.Windows.Application.Current.Resources.MergedDictionaries.Add(newTheme);

        }

        public static void ToggleTheme()
        {
            SetTheme(CurrentTheme == ThemeType.Light ? ThemeType.Dark : ThemeType.Light);
        }
    }
}
