using ElectronicMaps.Application.DTOs.Domain;
using ElectronicMaps.Application.Features.Workspace.Models;


namespace ElectronicMaps.Application.DTOs.Families
{
    /// <summary>
    /// Команда на создание нового семейства и первого компонента в этом семействе.
    /// Используется, когда по результатам анализа (AnalyzeAsync) выяснилось,
    /// что и компонент, и его семейство отсутствуют в базе.
    /// </summary>
    public class NewFamilyAndComponentDto
    {
        public string FamilyName { get; set; } = null!;

        public int FamilyFormId { get; set; }
        public string FamilyFormCode { get; set; } = "FORM_4";

        public string ComponentName { get; set; }
        public int ComponentFormId { get; set; }
        public string ComponentFormCode { get; set; }


        // Параметры — по коду/ID definitions + значения
        public IReadOnlyList<ParameterDto> FamilyParameters { get; init; } = Array.Empty<ParameterDto>();
        public IReadOnlyList<ParameterDto> ComponentParameters { get; init; } = Array.Empty<ParameterDto>();

        /// <summary>
        /// Опционально: исходный анализированный компонент, чтобы при необходимости
        /// логировать/связывать с исходными данными из XML.
        /// </summary>
        public ImportedRow? Source { get; init; }

        /// <summary>
        /// Удобный фабричный метод: собирает DTO из результата анализа и заполненных параметров.
        /// </summary>
        public static NewFamilyAndComponentDto FromAnalyzed(
            ImportedRow analyzed,
            IReadOnlyList<ParameterDto> familyParameters,
            IReadOnlyList<ParameterDto> componentParameters)
        {
            if (analyzed.FamilyFormId is null)
                throw new InvalidOperationException("FamilyFormTypeId должен быть задан для создания семейства.");

            if (analyzed.ComponentFormId is null)
                throw new InvalidOperationException("ComponentFormTypeId должен быть задан для создания компонента.");

            // Имя семейства: либо из анализа (Family), либо из DatabaseFamilyName, если оно есть.
            var familyName = analyzed.Family
                             ?? analyzed.DatabaseFamilyName
                             ?? throw new InvalidOperationException("Невозможно определить имя семейства.");

            return new NewFamilyAndComponentDto
            {
                FamilyName = familyName,
                FamilyFormId = analyzed.FamilyFormId.Value,

                ComponentName = analyzed.CleanName,

                ComponentFormId = analyzed.ComponentFormId.Value,

                FamilyParameters = familyParameters,
                ComponentParameters = componentParameters,
                Source = analyzed
            };

        }
    }
}
