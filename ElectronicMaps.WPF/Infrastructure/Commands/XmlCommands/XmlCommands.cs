using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Project;
using ElectronicMaps.Application.Services;
using ElectronicMaps.Application.Stores;
using ElectronicMaps.WPF.Services.Dialogs;
using ElectronicMaps.WPF.Services.Project;
using ElectronicMaps.WPF.ViewModels;
using Microsoft.EntityFrameworkCore.Metadata;
using Navigation.Core.Abstractions;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

                await _navigationService.NavigateAsync<WorkspaceViewModel>();

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
            var analyzedComponents = await _componentAnalysisService.AnalyzeAsync(stream);

            var enrichedComponents = analyzedComponents.Select(component =>
            {
                component.IsSelectedForImport = false;
                component.IsEdited = false;
                component.IsDirty = false;
                component.LastUpdatedUtc = DateTimeOffset.UtcNow;
                return component;
            }).ToList();

            _componentStore.ReplaceAll(enrichedComponents);
        }
    }
}
