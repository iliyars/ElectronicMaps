using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Infrastructure.Commands;
using ElectronicMaps.WPF.Infrastructure.Screens;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace ElectronicMaps.WPF.Features.Welcome
{
    public class WelcomeViewModel : BaseScreenViewModel
    {
        private readonly IComponentStore _componentStore;
        private readonly IAppCommands _commands;

        public WelcomeViewModel(IAppCommands commands)
        {
            _commands = commands;
        }

        // private readonly IRecentFilesService _recentFiles; // если понадобится
        // private readonly IOpenFileDialogService _openFileDialog; // если делаешь абстракцию поверх диалогов
        public IAsyncRelayCommand OpenXmlCommand => _commands.Xml.OpenXml;

        public ICommand ImportXmlCommand { get; }
        public ICommand OpenComponentsCommand { get; }
        public ICommand OpenFormsCommand { get; }
        public ICommand OpenSettingsCommand { get; }

        public bool HasComponents => _componentStore.GetAll().Any(); // или Count > 0
    }
}
