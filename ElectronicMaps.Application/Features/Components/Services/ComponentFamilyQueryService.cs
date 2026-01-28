using ElectronicMaps.Application.Abstractions.Queries.Families;
using ElectronicMaps.Application.DTOs.Families;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Application.Features.Components.Services
{
    public class ComponentFamilyQueryService : IComponentFamilyQueryService
    {
        private readonly IComponentFamilyReadRepository _families;
        private readonly ILogger<ComponentFamilyQueryService> _logger;

        public ComponentFamilyQueryService(
            IComponentFamilyReadRepository families,
            ILogger<ComponentFamilyQueryService> logger)
        {
            _families = families ?? throw new ArgumentNullException(nameof(families));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }
        /// <summary>
        /// Найти семейство по имени
        /// </summary>
        public async Task<ComponentFamilyLookupDto?> FindFamilyByNameAsync(
            string familyName,
            CancellationToken ct = default)
        {
            if (string.IsNullOrWhiteSpace(familyName))
            {
                throw new ArgumentException("Имя семейства не может быть пустым", nameof(familyName));
            }

            _logger.LogDebug("Поиск семейства по имени: {FamilyName}", familyName);

            try
            {
                var family = await _families.FindByNameAsync(familyName, ct);

                if (family == null)
                {
                    _logger.LogDebug("Семейство '{FamilyName}' не найдено", familyName);
                    return null;
                }

                _logger.LogDebug(
                    "Семейство найдено: Id={FamilyId}, Name={FamilyName}",
                    family.Id,
                    family.Name);

                return new ComponentFamilyLookupDto
                {
                    Id = family.Id,
                    Name = family.Name,
                    ComponentCount = family.ComponentCount,
                    FormTypeName = family.FormTypeName
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при поиске семейства по имени: {FamilyName}",
                    familyName);
                throw;
            }
        }

        /// <summary>
        /// Получить список всех семейств
        /// </summary>
        public async Task<IReadOnlyList<ComponentFamilyLookupDto>> GetAllFamiliesAsync(CancellationToken ct = default)
        {
            _logger.LogDebug("Получение списка всех семейств");

            try
            {
                var families = await _families.GetAllAsync(ct);

                var lookupDtos = families.Select(f => new ComponentFamilyLookupDto
                {
                    Id = f.Id,
                    Name = f.Name,
                    ComponentCount = f.ComponentCount,
                    FormTypeId = f.FormTypeId,
                    FormTypeName = f.FormTypeName,
                    FormCode = f.FormCode
                }).ToList();

                _logger.LogInformation(
                    "Получено {Count} семейств",
                    lookupDtos.Count);

                return lookupDtos;
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    ex,
                    "Ошибка при получении списка всех семейств");
                throw;
            }
        }
    }
}
