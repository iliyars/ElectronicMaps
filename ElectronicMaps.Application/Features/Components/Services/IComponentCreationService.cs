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
        public Task<ComponentCreationValidationResult> ValidateAsync(
           CreateComponentRequest request,
           CancellationToken ct = default);

        public Task<ComponentCreationResult> CreateComponentAsync(
            CreateComponentRequest request,
            CancellationToken ct = default);

    }
}
