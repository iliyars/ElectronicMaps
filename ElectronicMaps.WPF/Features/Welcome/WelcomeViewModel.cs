using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.ViewModels;
using ElectronicMaps.WPF.Services.Dialogs;
using MediatR;
using Microsoft.Extensions.Logging;
using Navigation.Core.Abstractions;
using System.Windows.Input;

namespace ElectronicMaps.WPF.Features.Welcome
{
    public partial class WelcomeViewModel : BaseScreenViewModel
    {
        private readonly IApplicationCommands _appCommands;

        [ObservableProperty]
        private bool _isBusy;

        [ObservableProperty]
        private string? _statusMessage;

        public WelcomeViewModel(IApplicationCommands appCommands)
        {
            _appCommands = appCommands ?? throw new ArgumentNullException(nameof(appCommands));
        }




        // private readonly IRecentFilesService _recentFiles; // если понадобится
        // private readonly IOpenFileDialogService _openFileDialog; // если делаешь абстракцию поверх диалогов

        /// <summary>
        /// Команда: Импортировать XML
        /// </summary>
        public ICommand ImportXmlCommand => _appCommands.ImportXmlCommand;
        public ICommand OpenComponentsCommand { get; }
        public ICommand OpenFormsCommand { get; }
        public ICommand OpenSettingsCommand { get; }

    }
}
