using CommunityToolkit.Mvvm.Input;
using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace ElectronicMaps.WPF.Infrastructure.Commands
{
    /// <summary>
    /// Глобальные команды приложения (Singleton)
    /// </summary>
    public interface IApplicationCommands
    {
        /// <summary>
        /// Команда: Импортировать XML файл
        /// </summary>
        IAsyncRelayCommand ImportXmlCommand { get; }

        /// <summary>
        /// Команда: Открыть проект
        /// </summary>
        IAsyncRelayCommand OpenProjectCommand { get; }

        /// <summary>
        /// Команда: Сохранить проект
        /// </summary>
        IAsyncRelayCommand SaveProjectCommand { get; }

        /// <summary>
        /// Команда: Сохранить проект как...
        /// </summary>
        IAsyncRelayCommand SaveProjectAsCommand { get; }

        /// <summary>
        /// Команда: Создать новый проект
        /// </summary>
        IAsyncRelayCommand NewProjectCommand { get; }

        /// <summary>
        /// Команда: Закрыть проект
        /// </summary>
        ICommand CloseProjectCommand { get; }

        /// <summary>
        /// Команда: Выход из приложения
        /// </summary>
        ICommand ExitCommand { get; }
    }
}
