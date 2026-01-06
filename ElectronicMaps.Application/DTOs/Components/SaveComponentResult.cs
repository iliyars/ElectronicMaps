using ElectronicMaps.Domain.Entities;
using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record SaveComponentResult
    {
        public required int ComponentId { get; init; }

        public required int ComponentFamilyId { get; init; }

        public required bool IsSuccess { get; init; }

        public bool FamilyWasCreated { get; init; }

        public VerificationStatus ComponentVerificationStatus { get; init; } =
            VerificationStatus.Unverified;

        public VerificationStatus FamilyVerificationStatus { get; init; } =
            VerificationStatus.Unverified;

        public string? ErrorMessage { get; init; }

        #region Factory Methods 

        /// <summary>
        /// Создать результат для успешного сохранения
        /// </summary>
        public static SaveComponentResult Success(
            int componentId,
            int familyId,
            bool familyWasCreated,
            VerificationStatus componentStatus = VerificationStatus.Unverified,
            VerificationStatus familyStatus = VerificationStatus.Unverified)
        {
            return new SaveComponentResult
            {
                ComponentId = componentId,
                ComponentFamilyId = familyId,
                IsSuccess = true,
                FamilyWasCreated = familyWasCreated,
                ComponentVerificationStatus = componentStatus,
                FamilyVerificationStatus = familyStatus
            };
        }

        /// <summary>
        /// Создать результат для ошибки
        /// </summary>
        public static SaveComponentResult Failure(string errorMessage)
        {
            return new SaveComponentResult
            {
                ComponentId = 0,
                ComponentFamilyId = 0,
                IsSuccess = false,
                ErrorMessage = errorMessage
            };
        }

        #endregion
    }
}
