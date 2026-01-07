using ElectronicMaps.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record ComponentCreationResult
    {
        public required bool IsSuccess { get; init; }
        public required int ComponentId { get; init; }
        public required int FamilyId { get; init; }
        public bool FamilyWasCreated { get; init; }
        public VerificationStatus ComponentVerificationStatus { get; init; } = VerificationStatus.Unverified;
        public VerificationStatus FamilyVerificationStatus { get; init; } = VerificationStatus.Unverified;
        public string? ErrorMessage { get; init; }

        public static ComponentCreationResult Success(
            int componentId,
            int familyId,
            bool familyWasCreated,
            VerificationStatus componentStatus = VerificationStatus.Unverified,
            VerificationStatus familyStatus = VerificationStatus.Unverified) => new()
       {
           IsSuccess = true,
           ComponentId = componentId,
           FamilyId = familyId,
           FamilyWasCreated = familyWasCreated,
           ComponentVerificationStatus = componentStatus,
           FamilyVerificationStatus = familyStatus
       };

        public static ComponentCreationResult Failure(string errorMessage) => new()
        {
            IsSuccess = false,
            ComponentId = 0,
            FamilyId = 0,
            ErrorMessage = errorMessage
        };


    }
}
