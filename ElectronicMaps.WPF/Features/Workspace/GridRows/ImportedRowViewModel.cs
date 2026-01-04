using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Application.Features.Workspace.Models;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.WPF.Features.Workspace.GridRows
{
    public class ImportedRowViewModel : ObservableObject
    {
        private ImportedRow Row { get; }

        public Guid RowId => Row.RowId;

        public string Name => Row.CleanName;
        public string? Family => Row.Family;
        public string Type => Row.Type;
        public int Quantity => Row.Quantity;

        public bool ComponentExistsInDatabase => Row.ComponentExistsInDatabase;
        public bool FamilyExistsInDatabase => Row.FamilyExistsInDatabase;

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set => SetProperty(ref _isSelected, value);
        }

        private bool _isDirty;
        public bool IsDirty
        {
            get => _isDirty;
            set => SetProperty(ref _isDirty, value);
        }

        private bool _isEdited;
        public bool IsEdited
        {
            get => _isEdited;
            set => SetProperty(ref _isEdited, value);
        }

        public ImportedRowViewModel(ImportedRow row)
        {
            Row = row ?? throw new ArgumentNullException(nameof(row));
        }
    }
}
