using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Project;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Features.Workspace;
using ElectronicMaps.WPF.Services.Dialogs;
using Navigation.Core.Abstractions;
using System.IO;


namespace ElectronicMaps.WPF.Infrastructure.Commands.XmlCommands
{
    public class XmlCommands : IXmlCommands
    {
        private readonly IDialogService _dialogService;
        private readonly IComponentAnalysisService _componentAnalysisService;
        private readonly IComponentStore _componentStore;
        private readonly IProjectSaveService _projectSaveService;
        private readonly INavigationService _navigationService;

        public IAsyncRelayCommand OpenXml { get; }

        public XmlCommands(IDialogService dialogService, IComponentAnalysisService componentAnalysisService, IComponentStore componentStore, IProjectSaveService projectSaveService, INavigationService navigationService)
        {
            _dialogService = dialogService;
            _componentAnalysisService = componentAnalysisService;
            _componentStore = componentStore;
            _projectSaveService = projectSaveService;
            _navigationService = navigationService;
            OpenXml = new AsyncRelayCommand(OpenXmlAsync);
        }

        public async Task OpenXmlAsync()
        {
            try
            {
                // 1. проверка на несохранённые данные
                if (HasUnsavedChanges() && !await HandleUnsavedChangesAsync())
                    return;

                // 2. диалог выбора файла
                var filePath = _dialogService.ShowOpenFileDialog(
                    "XML files (*.xml)|*.xml|AVS XML files (*.PE.XML)|*.PE.XML|All files (*.*)|*.*",
                    "Открыть файл компонентов");

                // пользователь нажал Cancel → выходим
                if (string.IsNullOrWhiteSpace(filePath))
                    return;

                //3.чтение файла
                await using var stream = File.OpenRead(filePath);

                //4.обработка XML
                   await ProcessXmlFileAsync(stream);

                await _navigationService.NavigateAsync<Features.Workspace.WorkspaceViewModel>();

            }
            catch (Exception ex)
            {
                _dialogService.ShowError("Ошибка при открытии XML", ex.Message);
            }
        }

        private bool HasUnsavedChanges()
        {
            return _componentStore.HasUnsavedChanges;
        }

        private async Task<bool> HandleUnsavedChangesAsync()
        {
            var result = _dialogService.ShowYesNoCancel(
                "У вас есть несохранённые изменения.\r\n" +
                "Сохранить текущий проект перед открытием нового файла?",
                "Несохранённые данные");

            switch (result)
            {
                case DialogResult.Cancel:
                    return false;

                case DialogResult.Yes:
                    var saved = await _projectSaveService.SaveCurrentProjectAsync();
                    return saved; // false → отмена

                case DialogResult.No:
                    return true;

                default:
                    return false;
            }
        }

        private async Task ProcessXmlFileAsync(Stream stream)
        {
            var importedRows = await _componentAnalysisService.AnalyzeAsync(stream);

            _componentStore.ReplaceImport(importedRows);
            _componentStore.InitializeWorkingDrafts();
            _componentStore.RebuildViewsByForms();
        }
    }
}
