using CommunityToolkit.Mvvm.ComponentModel;
using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.WPF.ViewModels
{
    public class EditableParameterViewModel : ObservableObject
    {
        public string Code { get; }
        public string DisplayName { get; }
        public string? Unit { get; }

        // универсальное поле для ввода (UI всегда работает со строками)
        private string? _value;
        public string? Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        // можно добавить тип значения, если хочешь строгую валидацию
        public string ValueKind { get; } // "String", "Int", "Double", "WithPins" и т.п.

        public EditableParameterViewModel(ParameterDto dto)
        {
            Code = dto.Code;
            DisplayName = dto.DisplayName;
            Unit = dto.Unit;

            // берём первое непустое значение для отображения
            _value = dto.StringValue
                     ?? dto.DoubleValue?.ToString()
                     ?? dto.IntValue?.ToString();
        }

        // метод для конвертации обратно в ParameterDto перед сохранением
        public ParameterDto ToDto()
        {
            // здесь можно аккуратно парсить обратно строки в int/double, исходя из ValueKind
            return new ParameterDto
            {
                Code = Code,
                DisplayName = DisplayName,
                Unit = Unit,
                StringValue = _value, // простейший вариант: храним как строку
                                      // по-хорошему — разобрать _value в DoubleValue/IntValue
            };
        }
    }
}
