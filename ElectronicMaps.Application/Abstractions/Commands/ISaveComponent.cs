using ElectronicMaps.Application.DTOs.Components;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Abstractions.Commands
{
    /// <summary>
    /// Сохранение компонента в БД.
    /// </summary>
    public interface ISaveComponent
    {
        /// <summary>
        /// CСохраняет компонент и семейство (при необходимости) в БД.
        /// </summary>
        /// <param name="request">Данные компонента для сохранения</param>
        /// <param name="ct">Токен отмены операции</param>
        /// <returns>езультат с ID созданных сущностей</returns>
        Task<SaveComponentResult> SaveAsync(SaveComponentRequest request, CancellationToken ct);
    }
}
