using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.Services.Dialogs
{
    public enum DialogResult
    {
        None,
        OK,
        Cancel,
        Yes,
        No
    }

    public interface IDialogService
    {
        #region File Dialogs
        /// <summary>
        /// Показать диалог открытия файла
        /// </summary>
        string? ShowOpenFileDialog(string filter, string title);

        /// <summary>
        /// Показать диалог сохранения файла
        /// </summary>
        string? ShowSaveFileDialog(string filter, string title);

        #endregion

        #region Message Dialogs

        /// <summary>Показать простое сообщение (OK).</summary>
        void ShowMessage(string message, string caption = "Сообщение");

        /// <summary>Показать ошибку.</summary>
        void ShowError(string message, string caption = "Ошибка");

        /// <summary>Показать предупреждение.</summary>
        void ShowWarning(string message, string caption = "Предупреждение");

        /// <summary>Показать диалог Yes / No / Cancel.</summary>
        DialogResult ShowYesNoCancel(string message, string caption);

        /// <summary>Показать диалог Yes / No.</summary>
        DialogResult ShowYesNo(string message, string caption);

        /// <summary>Показать диалог OK / Cancel.</summary>
        DialogResult ShowOkCancel(string message, string caption);

        #endregion

        #region Custom Dialogs

        /// <summary>
        /// Показать модальное окно с ViewModel
        /// </summary>
        /// <typeparam name="TViewModel">Тип ViewModel</typeparam>
        /// <param name="viewModel">ViewModel для окна</param>
        /// <returns>true если пользователь нажал OK/Save, false если Cancel</returns>
        Task<bool> ShowDialogAsync<TViewModel>(TViewModel viewModel) where TViewModel : class;

        /// <summary>
        /// Показать сообщение об ошибке (асинхронно)
        /// </summary>
        Task ShowErrorAsync(string message, string caption = "Ошибка");

        /// <summary>
        /// Показать информационное сообщение (асинхронно)
        /// </summary>
        Task ShowMessageAsync(string message, string caption = "Сообщение");

        // <summary>
        /// Показать диалог подтверждения (асинхронно)
        /// </summary>
        /// <returns>true если пользователь подтвердил (Yes), false если отменил (No)</returns>
        Task<bool> ShowConfirmationAsync(string message, string caption = "Подтверждение");

        /// <summary>
        /// Показать кастомный диалог с асинхронной инициализацией
        /// </summary>
        bool? ShowDialogWithInitialization<TViewModel>(
            TViewModel viewModel,
            Func<TViewModel, Task> initializeAsync) where TViewModel : class;

        #endregion
    }

}
