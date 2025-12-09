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
        string? ShowOpenFileDialog(string filter, string title);
        string? ShowSaveFileDialog(string filter, string title);

        // ----------------------- MESSAGE DIALOGS -----------------------

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
    }
}
