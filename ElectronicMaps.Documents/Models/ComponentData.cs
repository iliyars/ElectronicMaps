using System;
using System.Collections.Generic;
using System.Text;

namespace ElectronicMaps.Documents.Models
{
    public class ComponentData
    {
        /// <summary>
        /// Параметры компонента.
        /// <remarks>
        /// Ключ = код параметра из JSON cхемы (filedCode)
        /// Значение = значение параметра.
        /// </remarks>
        /// </summary>
        public Dictionary<string, string> Parameters { get; set; } = new Dictionary<string, string>();

        public static ComponentData FromDictionary(Dictionary<string, string> parameters)
        {
            return new ComponentData
            {
                Parameters = new Dictionary<string, string>(parameters, StringComparer.OrdinalIgnoreCase)
            };
        }
        /// <summary>
        /// Получить значение параметра по коду.
        /// </summary>
        /// <param name="fieldCode"></param>
        /// <returns>Значение параметра или null если не найдено</returns>
        public string? GetValue(string fieldCode)
        {
            return Parameters.TryGetValue(fieldCode, out var value) ? value : null;
        }

        /// <summary>
        /// Установить значение параметра
        /// </summary>
        /// <param name="fieldCode">Код параметра</param>
        /// <param name="value">Знысение</param>
        public void SetValue(string fieldCode, string value)
        {
            Parameters[fieldCode] = value;
        }

        /// <summary>
        /// Проверить наличие параметра
        /// </summary>
        /// <param name="fieldCode">Код параметра</param>
        /// <returns>true если параметр существует</returns>
        public bool HasParameter(string fieldCode)
        {
            return Parameters.ContainsKey(fieldCode);
        }

        /// <summary>
        /// Количество параметров
        /// </summary>
        public int Count => Parameters.Count;

    }
}
