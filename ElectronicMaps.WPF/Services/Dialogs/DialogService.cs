using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicMaps.WPF.Services.Dialogs
{
    public class DialogService : IDialogService
    {
        public string? ShowOpenFileDialog(string filter, string title)
        {
            var dlg = new OpenFileDialog
            {
                Filter = filter,
                Title = title,
                CheckFileExists = true,
                Multiselect = false
            };

            var owner = GetOwnerWindow();
            var result = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();

            return result == true ? dlg.FileName : null;
        }

        public string? ShowSaveFileDialog(string filter, string title)
        {
            var dlg = new SaveFileDialog
            {
                Filter = filter,
                Title = title,
                AddExtension = true,
                OverwritePrompt = true
            };

            var owner = GetOwnerWindow();
            var result = owner != null ? dlg.ShowDialog(owner) : dlg.ShowDialog();

            return result == true ? dlg.FileName : null;
        }


        // ---------- MESSAGE DIALOGS ----------

        public void ShowMessage(string message, string caption = "Сообщение")
        {
            ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public void ShowError(string message, string caption = "Ошибка")
        {
            ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public void ShowWarning(string message, string caption = "Предупреждение")
        {
            ShowMessageBox(message, caption, MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        public DialogResult ShowYesNoCancel(string message, string caption)
        {
            var result = ShowMessageBox(message, caption, MessageBoxButton.YesNoCancel, MessageBoxImage.Warning);
            return ConvertResult(result);
        }

        public DialogResult ShowYesNo(string message, string caption)
        {
            var result = ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return ConvertResult(result);
        }

        public DialogResult ShowOkCancel(string message, string caption)
        {
            var result = ShowMessageBox(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return ConvertResult(result);
        }


        private Window? GetOwnerWindow()
        {
            // Можно улучшить, если у тебя есть логика активного окна.
            return System.Windows.Application.Current?.MainWindow;
        }
        private MessageBoxResult ShowMessageBox(
            string message,
            string caption,
            MessageBoxButton buttons,
            MessageBoxImage image)
        {
            var owner = GetOwnerWindow();
            return owner != null
                ? MessageBox.Show(owner, message, caption, buttons, image)
                : MessageBox.Show(message, caption, buttons, image);
        }

        private static DialogResult ConvertResult(MessageBoxResult result) =>
            result switch
            {
                MessageBoxResult.OK => DialogResult.OK,
                MessageBoxResult.Yes => DialogResult.Yes,
                MessageBoxResult.No => DialogResult.No,
                MessageBoxResult.Cancel => DialogResult.Cancel,
                _ => DialogResult.None
            };
    }
}
