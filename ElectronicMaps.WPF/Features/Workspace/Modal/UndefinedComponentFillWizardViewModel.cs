using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.Features.Workspace.Models;
using ElectronicMaps.Application.Features.Workspace.Services;
using ElectronicMaps.WPF.Features.Workspace.Parameters;
using System.Collections.ObjectModel;

namespace ElectronicMaps.WPF.Features.Workspace.Modal
{
    public partial class UndefinedComponentFillWizardViewModel : ObservableObject
    {
        private readonly IUndefinedComponentFillService _service;

        public Guid DraftId { get; }

        [ObservableProperty]
        private int step = 1;

        public ParametersEditorViewModel FamilyEditor { get; private set; } = null!;
        public ParametersEditorViewModel ComponentEditor { get; private set; } = null!;

        public ObservableCollection<FormTypeDto> FormTypes { get; } = new();

        [ObservableProperty]
        private FormTypeDto? selectedFormType;

        [ObservableProperty]
        private string familyName = "";

        public IRelayCommand NextCommand { get; }
        public IRelayCommand BackCommand { get; }

        public IRelayCommand FinishCommand { get; }


        public UndefinedComponentFillWizardViewModel(
            Guid draftId,
            IUndefinedComponentFillService service)
        {
            DraftId = draftId;
            _service = service;


            NextCommand = new RelayCommand(() => Step = 2, () => Step == 1);
            NextCommand = new RelayCommand(() => Step = 1, () => Step == 2);
            FinishCommand = new AsyncRelayCommand(FinishAsync, () => Step == 2 && SelectedFormType != null);
        }

        public async Task InitializeAsync(CancellationToken ct)
        {
            var familyDefs = await _service.GetFamilyDefinitionsAsync(ct);
            FamilyEditor = new Features.Workspace.Parameters.ParametersEditorViewModel(familyDefs);

            var forms = await _service.GetComponentFormTypesAsync(ct);
            FormTypes.Clear();
            foreach(var f in forms.Where(x => x.Code != WorkspaceViewKeys.FamilyFormCode))
                FormTypes.Add(f);

            OnPropertyChanged(nameof(FamilyEditor));
        }

        partial void OnSelectedFormTypeChanged(FormTypeDto? oldValue, FormTypeDto? newValue)
        {
            _ = LoadComponentDefsAsync(newValue);
            FinishCommand.NotifyCanExecuteChanged();
        }

        private async Task LoadComponentDefsAsync(FormTypeDto? form)
        {
            if (form is null) return;

            var defs = await _service.GetComponentDefinitionsAsync(form.Code, CancellationToken.None);
            ComponentEditor = new Features.Workspace.Parameters.ParametersEditorViewModel(defs);
            OnPropertyChanged(nameof(ComponentEditor));
        }
        
        private async Task FinishAsync()
        {
            if (SelectedFormType is null) return;
            
            await _service.SaveAsync(
                draftId: DraftId,
                familyName: FamilyName,
                familyParameters: FamilyEditor.BuildInputs(),
                componentFormTypeCode: SelectedFormType.Code,
                componentParameters: ComponentEditor.BuildInputs(),
                ct: CancellationToken.None);
        }


    }
}
