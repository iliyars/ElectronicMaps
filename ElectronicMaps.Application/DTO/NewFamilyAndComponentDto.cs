using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO
{
    /// <summary>
    /// Команда на создание нового семейства и первого компонента в этом семействе.
    /// Используется, когда по результатам анализа (AnalyzeAsync) выяснилось,
    /// что и компонент, и его семейство отсутствуют в базе.
    /// </summary>
    public class NewFamilyAndComponentDto
    {
        public string FamilyName { get; set; } = null!;

        public int FamilyFormTypeId { get; set; }

        public string ComponentName { get; set; }

        public int ComponentFormTypeId { get; set; }


        // Параметры — по коду/ID definitions + значения
        public IReadOnlyList<ParameterDto> FamilyParameters { get; init; } = Array.Empty<ParameterDto>();
        public IReadOnlyList<ParameterDto> ComponentParameters { get; init; } = Array.Empty<ParameterDto>();

        /// <summary>
        /// Опционально: исходный анализированный компонент, чтобы при необходимости
        /// логировать/связывать с исходными данными из XML.
        /// </summary>
        public AnalyzedComponentDto? Source { get; init; }

        /// <summary>
        /// Удобный фабричный метод: собирает DTO из результата анализа и заполненных параметров.
        /// </summary>
        public static NewFamilyAndComponentDto FromAnalyzed(
            AnalyzedComponentDto analyzed,
            IReadOnlyList<ParameterDto> familyParameters,
            IReadOnlyList<ParameterDto> componentParameters)
        {
            if (analyzed.FamilyFormTypeId is null)
                throw new InvalidOperationException("FamilyFormTypeId должен быть задан для создания семейства.");

            if (analyzed.ComponentFormTypeId is null)
                throw new InvalidOperationException("ComponentFormTypeId должен быть задан для создания компонента.");

            // Имя семейства: либо из анализа (Family), либо из DatabaseFamilyName, если оно есть.
            var familyName = analyzed.Family
                             ?? analyzed.DatabaseFamilyName
                             ?? throw new InvalidOperationException("Невозможно определить имя семейства.");

            return new NewFamilyAndComponentDto
            {
                FamilyName = familyName,
                FamilyFormTypeId = analyzed.FamilyFormTypeId.Value,

                ComponentName = analyzed.CleanName,

                ComponentFormTypeId = analyzed.ComponentFormTypeId.Value,

                FamilyParameters = familyParameters,
                ComponentParameters = componentParameters,

                Source = analyzed
            };

        }
    }
}
