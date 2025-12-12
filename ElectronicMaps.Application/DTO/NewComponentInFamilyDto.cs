using ElectronicMaps.Domain.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTO
{
    /// <summary>
    /// Команда на создание нового компонента в существующем семействе.
    /// Используется, когда по результатам анализа семейство найдено в БД,
    /// а компонента с таким именем ещё нет.
    /// </summary>
    public class NewComponentInFamilyDto
    {
        public int FamilyId { get; init; }                  // уже существующее семейство
        public string FamilyFormCode { get; init; } = "FORM_4";
        public int FamilyFormTypeId { get; init; }
        public string ComponentName { get; init; } = null!;
        public int ComponentFormTypeId { get; init; }
        public string ComponentFormCode { get; init; }


        public IReadOnlyList<ParameterDto> ComponentParameters { get; init; } = Array.Empty<ParameterDto>();

        /// <summary>
        /// Флаг, нужно ли обновлять параметры семейства в рамках этой операции.
        /// Если false, UpdatedFamilyParameters игнорируется.
        /// </summary>:
        public bool UpdateFamilyParameters { get; init; }
        public IReadOnlyList<ParameterDto> UpdatedFamilyParameters { get; init; } = Array.Empty<ParameterDto>();

        /// <summary>
        /// Исходный анализированный компонент (опционально, для трассировки/логирования).
        /// </summary>
        public AnalyzedComponentDto? Source { get; init; }

        public static NewComponentInFamilyDto FromAnalyzed(
        AnalyzedComponentDto analyzed,
        IReadOnlyList<ParameterDto> componentParameters,
        bool updateFamilyParameters = false,
        IReadOnlyList<ParameterDto>? updatedFamilyParameters = null)
        {
            if (analyzed.DatabaseFamilyId is null)
                throw new InvalidOperationException("DatabaseFamilyId должен быть задан для создания компонента в семействе.");

            if (analyzed.ComponentFormId is null)
                throw new InvalidOperationException("ComponentFormTypeId должен быть задан для создания компонента.");

            return new NewComponentInFamilyDto
            {
                FamilyId = analyzed.DatabaseFamilyId.Value,

                ComponentName = analyzed.CleanName,
                ComponentFormCode = analyzed.ComponentFormCode,

                ComponentFormTypeId = analyzed.ComponentFormId.Value,

                ComponentParameters = componentParameters,

                UpdateFamilyParameters = updateFamilyParameters,
                UpdatedFamilyParameters = updatedFamilyParameters ?? Array.Empty<ParameterDto>(),

                Source = analyzed
            };
        }
    }
}
