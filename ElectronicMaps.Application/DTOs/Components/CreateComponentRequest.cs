using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.DTOs.Components
{
    /// <summary>
    /// Режим работы с семейством
    /// </summary>
    public enum FamilySelectionMode
    {
        /// <summary>
        /// Создать новое семейство
        /// </summary>
        CreateNew,

        /// <summary>
        /// Использовать существующее семейство
        /// </summary>
        UseExisting
    }


    /// <summary>
    /// Запрос на создание компонента (используется в UI)
    /// </summary>
    public record CreateComponentRequest
    {
        public required string ComponentName { get; init; }

        #region Семейство

        /// <summary>
        /// Режим работы с семейством
        /// </summary>
        public FamilySelectionMode FamilyMode { get; init; }

        /// <summary>
        /// ID существующего семейства (если FamilyMode = UseExisting)
        /// </summary>
        public int? ExistingFamilyId { get; init; }

        /// <summary>
        /// Имя нового семейства (если FamilyMode = CreateNew)
        /// </summary>
        public string? NewFamilyName { get; init; }

        /// <summary>
        /// Параметры семейства (только если FamilyMode = CreateNew)
        /// </summary>
        public IReadOnlyList<ParameterValueInput>? FamilyParameters { get; init; }

        #endregion

        #region Компонент

        /// <summary>
        /// Код формы компонента (обязательно)
        /// </summary>
        public required string ComponentFormTypeCode { get; init; }

        /// <summary>
        /// Параметры компонента
        /// </summary>
        public IReadOnlyList<ParameterValueInput>? ComponentParameters { get; init; }

        #endregion

        #region Аудит

        /// <summary>
        /// ID пользователя
        /// </summary>
        public int? UserId { get; init; }

        #endregion

    }
}
