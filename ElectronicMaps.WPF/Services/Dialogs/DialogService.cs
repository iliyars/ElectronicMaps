using ElectronicMaps.WPF.Features.Workspace.Components;
using Microsoft.Extensions.Logging;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace ElectronicMaps.WPF.Services.Dialogs
{
    public class DialogService : IDialogService
    {
        private readonly ILogger<DialogService> _logger;

        public DialogService(ILogger<DialogService> logger)
        {
            _logger = logger;
        }

        #region File Dialogs

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
        #endregion

        #region Message Dialogs (Синхронные)

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
            return MapMessageBoxResult(result);
        }

        public DialogResult ShowYesNo(string message, string caption)
        {
            var result = ShowMessageBox(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
            return MapMessageBoxResult(result);
        }

        public DialogResult ShowOkCancel(string message, string caption)
        {
            var result = ShowMessageBox(message, caption, MessageBoxButton.OKCancel, MessageBoxImage.Question);
            return MapMessageBoxResult(result);
        }
        private static DialogResult MapMessageBoxResult(MessageBoxResult result) =>
           result switch
           {
               MessageBoxResult.OK => DialogResult.OK,
               MessageBoxResult.Yes => DialogResult.Yes,
               MessageBoxResult.No => DialogResult.No,
               MessageBoxResult.Cancel => DialogResult.Cancel,
               _ => DialogResult.None
           };


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

        #endregion

        #region Message Dialogs (Асинхронные)

        public Task ShowErrorAsync(string message, string caption = "Ошибка")
        {
            _logger.LogDebug("Показ ошибки: {Caption} - {Message}", caption, message);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    message,
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Error);
            });

            return Task.CompletedTask;
        }

        public Task ShowMessageAsync(string message, string caption = "Сообщение")
        {
            _logger.LogDebug("Показ сообщения: {Caption} - {Message}", caption, message);

            System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                MessageBox.Show(
                    message,
                    caption,
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
            });

            return Task.CompletedTask;
        }

        public Task<bool> ShowConfirmationAsync(string message, string caption = "Подтверждение")
        {
            _logger.LogDebug("Показ подтверждения: {Caption} - {Message}", caption, message);

            var result = System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                var messageBoxResult = MessageBox.Show(
                    message,
                    caption,
                    MessageBoxButton.YesNo,
                    MessageBoxImage.Question);

                return messageBoxResult == MessageBoxResult.Yes;
            });

            return Task.FromResult(result);
        }



        #endregion

        #region Custom Dialogs

        public Task<bool> ShowDialogAsync<TViewModel> (TViewModel viewModel) where TViewModel : class
        {
            if (viewModel == null)
                throw new ArgumentNullException(nameof(viewModel));

            _logger.LogDebug("Открытие диалога для ViewModel: {ViewModelType}", typeof(TViewModel).Name);

            return System.Windows.Application.Current.Dispatcher.Invoke(() =>
            {
                try
                {
                    Window? dialog = CreateDialogForViewModel(viewModel);

                    if (dialog == null)
                    {
                        _logger.LogWarning("Не удалось создать диалог для {ViewModelType}", typeof(TViewModel).Name);
                        return Task.FromResult(false);
                    }

                    // Устанавливаем DataContext
                    dialog.DataContext = viewModel;

                    // Устанавливаем Owner (главное окно приложения)
                    if (System.Windows.Application.Current.MainWindow?.IsVisible == true)
                    {
                        dialog.Owner = System.Windows.Application.Current.MainWindow;
                    }

                    // Показываем модально
                    var result = dialog.ShowDialog();

                    _logger.LogDebug("Диалог закрыт с результатом: {Result}", result);

                    return Task.FromResult(result == true);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Ошибка при открытии диалога");
                    throw;
                }
            });
        }

        /// <summary>
        /// Создать окно для ViewModel
        /// Маппинг ViewModel → View
        /// </summary>
        private Window? CreateDialogForViewModel<TViewModel>(TViewModel viewModel)
        {
            var viewModelTypeName = typeof(TViewModel).Name;

            return viewModelTypeName switch
            {
                "CreateComponentViewModel" => new CreateComponentDialog(),
                // Добавьте другие маппинги здесь:
                // "EditComponentViewModel" => new EditComponentDialog(),
                // "SettingsViewModel" => new SettingsDialog(),
                _ => null
            };
        }


        #endregion


       
       
    }
}
