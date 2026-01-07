using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.Features.Import;
using ElectronicMaps.WPF.Features.Workspace;
using ElectronicMaps.WPF.Services.Dialogs;
using MediatR;
using Microsoft.Extensions.Logging;
using Navigation.Core.Abstractions;
using System.Windows.Input;

namespace ElectronicMaps.WPF.Infrastructure.Commands
{
    public class ApplicationCommands : IApplicationCommands
    {
        private readonly IMediator _mediator;
        private readonly INavigationService _navigationService;
        private readonly IDialogService _dialogService;
        private readonly ILogger<ApplicationCommands> _logger;

        public ApplicationCommands(
        IMediator mediator,
        INavigationService navigationService,
        IDialogService dialogService,
        ILogger<ApplicationCommands> logger)
        {
            _mediator = mediator ?? throw new ArgumentNullException(nameof(mediator));
            _navigationService = navigationService ?? throw new ArgumentNullException(nameof(navigationService));
            _dialogService = dialogService ?? throw new ArgumentNullException(nameof(dialogService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            // Инициализация команд
            ImportXmlCommand = new AsyncRelayCommand(ImportXmlAsync);
            //OpenProjectCommand = new AsyncRelayCommand(OpenProjectAsync);
            //SaveProjectCommand = new AsyncRelayCommand(SaveProjectAsync);
            //SaveProjectAsCommand = new AsyncRelayCommand(SaveProjectAsAsync);
            //NewProjectCommand = new AsyncRelayCommand(NewProjectAsync);
            //CloseProjectCommand = new RelayCommand(CloseProject);
            //ExitCommand = new RelayCommand(Exit);
        }

        #region Commands

        public IAsyncRelayCommand ImportXmlCommand { get; }
        public IAsyncRelayCommand OpenProjectCommand { get; }
        public IAsyncRelayCommand SaveProjectCommand { get; }
        public IAsyncRelayCommand SaveProjectAsCommand { get; }
        public IAsyncRelayCommand NewProjectCommand { get; }
        public ICommand CloseProjectCommand { get; }
        public ICommand ExitCommand { get; }

        #endregion

        #region Implementation

        private async Task ImportXmlAsync()
        {
            try
            {
                _logger.LogInformation("Команда: Импорт XML");

                // Диалог выбора файла
                var filePath = _dialogService.ShowOpenFileDialog(
                    "XML файлы (*.xml)|*.xml|Все файлы (*.*)|*.*",
                    "Открыть XML файл");

                if (string.IsNullOrWhiteSpace(filePath))
                {
                    _logger.LogDebug("Пользователь отменил выбор файла");
                    return;
                }

                // Выполнение через MediatR
                var result = await _mediator.Send(new ImportXmlCommand(filePath));

                // Обработка результата
                if (result.IsSuccess)
                {
                    _logger.LogInformation(
                        "XML успешно импортирован: {Count} компонентов",
                        result.ComponentsImported);

                    _dialogService.ShowMessage(
                        $"Импортировано компонентов: {result.ComponentsImported}",
                        "Импорт завершён");

                    await _navigationService.NavigateAsync<WorkspaceViewModel>();

                }
                else
                {
                    _logger.LogWarning("Ошибка импорта: {Error}", result.ErrorMessage);
                    _dialogService.ShowError(result.ErrorMessage ?? "Неизвестная ошибка", "Ошибка импорта");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Ошибка при импорте XML");
                _dialogService.ShowError($"Ошибка: {ex.Message}", "Ошибка");
            }
        }
        #endregion

    }
}

