using ElectronicMaps.Application.DTOs.Components;
using ElectronicMaps.Application.DTOs.Families;
using ElectronicMaps.Application.DTOs.Forms;
using ElectronicMaps.Application.DTOs.Parameters;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    /// <summary>
    /// Сервис для создания компонента с интерактивным заполнением параметров
    /// Используется для UI workflow: добавление компонента → заполнение формы → сохранение
    /// </summary>
    public interface IComponentCreationService
    {
        #region 1. Инициализация (получение данных для UI)
        /// <summary>
        /// Получить параметры семейства для заполнения (одинаковые для всех компонентов)
        /// Используется для построения формы семейства в UI
        /// </summary>
        /// <returns>Список определений параметров семейства (упорядоченный по Order)</returns>
        Task<IReadOnlyList<ParameterDefinitionDto>> GetFamilyParameterDefinitionsAsync(
            CancellationToken ct = default);

        /// Получить список доступных форм компонента для выбора в ComboBox
        /// </summary>
        /// <returns>Список типов форм (FormType)</returns>
        Task<IReadOnlyList<FormTypeDto>> GetAvailableComponentFormsAsync(
            CancellationToken ct = default);

        /// <summary>
        /// Получить параметры компонента для заполнения (зависят от выбранной формы)
        /// Вызывается после выбора FormType в ComboBox
        /// </summary>
        /// <param name="formTypeCode">Код формы компонента (например, "FORM_FAMILY_64")</param>
        /// <returns>Список определений параметров компонента (упорядоченный по Order)</returns>
        Task<IReadOnlyList<ParameterDefinitionDto>> GetComponentParameterDefinitionsAsync(
            string formTypeCode,
            CancellationToken ct = default);
        #endregion

        #region 2. Проверка существования семейства

        /// <summary>
        /// Проверить существует ли семейство с указанным именем
        /// Используется для выбора между созданием нового семейства и использованием существующего
        /// </summary>
        /// <param name="familyName">Имя семейства для проверки</param>
        /// <returns>Информация о семействе если существует, иначе null</returns>
        Task<ComponentFamilyLookupDto?> FindFamilyByNameAsync(
            string familyName,
            CancellationToken ct = default);

        /// <summary>
        /// Получить список всех существующих семейств для выбора в UI
        /// </summary>
        /// <returns>Список семейств</returns>
        Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllFamiliesAsync(
            CancellationToken ct = default);

        #endregion

        #region 3. Валидация (перед сохранением)
        /// <summary>
        /// Валидировать данные перед сохранением компонента
        /// </summary>
        /// <param name="request">Запрос на создание компонента</param>
        /// <returns>Результат валидации</returns>
        Task<ComponentCreationValidationResult> ValidateAsync(
            CreateComponentRequest request,
            CancellationToken ct = default);
        #endregion

        #region 4. Сохранение компонента

        /// <summary>
        /// Создать компонент с указанными параметрами
        /// </summary>
        /// <param name="request">Данные для создания компонента</param>
        /// <returns>Результат создания с ID компонента и семейства</returns>
        Task<ComponentCreationResult> CreateComponentAsync(
            CreateComponentRequest request,
            CancellationToken ct = default);

        #endregion

    }
}
