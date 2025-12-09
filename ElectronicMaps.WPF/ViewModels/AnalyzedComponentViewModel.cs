using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.DTO;
using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels
{
    public class AnalyzedComponentViewModel : ObservableObject
    {
        private readonly AnalyzedComponentDto _analyzedComponent;

        private bool _parametersLoaded;

        public string RawName => _analyzedComponent.RawName;
        public string Type => _analyzedComponent.Type;
        public string Family => _analyzedComponent.Family;
        public string CleanName => _analyzedComponent.CleanName;
        public int Quantity =>  _analyzedComponent.Quantity;
        public string? Designators => _analyzedComponent.Designators;
        public bool ExistsInDatabase => _analyzedComponent.ExistsInDatabase;

        public bool CanShowParameters => ExistsInDatabase;


        public bool _isParametersLoading;
        public bool IsParametersLoading
        {
            get => _isParametersLoading;
            private set => SetProperty(ref _isParametersLoading, value);
        }

        private ObservableCollection<ParameterDto> _parameters = new();
        public ObservableCollection<ParameterDto> Parameters
        {
            get => _parameters;
            private set => SetProperty(ref _parameters, value);
        }

        public AnalyzedComponentViewModel(AnalyzedComponentDto analyzedComponent)
        {
            _analyzedComponent = analyzedComponent;
        }

        public async Task LoadParameterAsync(IFormQueryService formQueryService, CancellationToken ct = default)
        {
            if (!ExistsInDatabase || _parametersLoaded)
            {
                return;
            }
            IsParametersLoading = true;
            try
            {
                var list = await formQueryService.GetComponentFormAsync(CleanName, ct);
                _parametersLoaded = true;
                Parameters = new ObservableCollection<ParameterDto>(list);
            }
            catch (Exception)
            {

            }
            finally
            {
                IsParametersLoading = false;
            }
        }
            
        


    }
}
