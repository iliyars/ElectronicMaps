using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.DTOs.Components
{
    public record ComponentCreationValidationResult
    {
        public required bool IsValid { get; init; }
        public IReadOnlyList<string> Errors { get; init; } = [];
        public IReadOnlyList<string> Warnings { get; init; } = [];

        public static ComponentCreationValidationResult Success() => new() { IsValid = true };

        public static ComponentCreationValidationResult Failure(params string[] errors) => new()
        {
            IsValid = false,
            Errors = errors
        };
    }
}
