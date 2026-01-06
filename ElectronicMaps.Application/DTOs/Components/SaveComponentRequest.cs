using ElectronicMaps.Application.DTOs.Parameters;
using ElectronicMaps.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record SaveComponentRequest
    {
        public required string ComponentName { get; set; }

        public required string ComponentFormTypeCode { get; set; }

        public IReadOnlyList<ParameterValueInput>? ComponentParameters { get; set; }

        #region Семейство (Family)

        public int? ExistingFamilyId { get; set; }

        public string? FamilyName { get; set; }

        public string? FamilyFormTypeCode { get; set; }

        public IReadOnlyList<ParameterValueInput>? FamilyParameters { get; set; }

        #endregion

        #region Аудит

        /// <summary>
        /// ID пользователя, создающего компонент (опционально)
        /// </summary>
        public int? CreatedByUserId { get; set; }

        #endregion

        #region Валидация 

        public bool IsValid(out string? errorMessage)
        {
            // Обязательные поля
            if (string.IsNullOrWhiteSpace(ComponentName))
            {
                errorMessage = "Имя компонента обязательно";
                return false;
            }

            if (string.IsNullOrWhiteSpace(ComponentFormTypeCode))
            {
                errorMessage = "Код формы компонента обязателен";
                return false;
            }

            // Семейство: либо существующее, либо новое
            if (ExistingFamilyId == null && string.IsNullOrWhiteSpace(FamilyName))
            {
                errorMessage = "Необходимо указать либо ID существующего семейства, либо имя нового";
                return false;
            }

            // Для нового семейства нужен FormTypeCode
            if (ExistingFamilyId == null && string.IsNullOrWhiteSpace(FamilyFormTypeCode))
            {
                errorMessage = "Для нового семейства необходимо указать код формы";
                return false;
            }

            errorMessage = null;
            return true;
        }

        #endregion
    }
}
