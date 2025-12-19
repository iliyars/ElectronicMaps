using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Utils
{
    public static class DesignatorHelper
    {
        // Пример токенов: "D1", "R12", "C003", "VD5"
        // Группа 1 = prefix (буквы), группа 2 = number
        private static readonly Regex TokenRegex =
            new(@"^\s*([A-Za-zА-Яа-я]+)\s*0*([0-9]+)\s*$", RegexOptions.Compiled);

        // Пример диапазона: "D1-D4" (префикс должен совпадать)
        private static readonly Regex RangeRegex =
            new(@"^\s*([A-Za-zА-Яа-я]+)\s*0*([0-9]+)\s*-\s*([A-Za-zА-Яа-я]+)?\s*0*([0-9]+)\s*$",
                RegexOptions.Compiled);
        /// <summary>
        /// Парсит строку designators из XML в нормализованный список токенов (D1, D2, ...).
        /// Поддерживает списки и диапазоны: "D1,D2" / "D1-D4" / "D1, D3-D5".
        /// </summary>
        public static IReadOnlyList<string> Expand(string? input)
        {
            if (string.IsNullOrWhiteSpace(input))
                return Array.Empty<string>();

            var result = new List<(string Prefix, int Number)>();

            foreach (var part in SplitParts(input))
            {
                if (TryParseRange(part, out var rangePrefix, out var start, out var end))
                {
                    var step = start <= end ? 1 : -1;
                    for (int n = start; n != end + step; n += step)
                        result.Add((rangePrefix, n));

                    continue;
                }

                if (TryParseToken(part, out var prefix, out var number))
                {
                    result.Add((prefix, number));
                    continue;
                }

                // Если встретили "нестандартный" элемент (без числа), можно:
                // 1) игнорировать, 2) бросить исключение, 3) сохранить как есть.
                // Я предлагаю сохранять как "сырой" токен (но без участия в split по числам).
                // Для простоты: сохраним как отдельный "prefix", number = int.MinValue
                var raw = part.Trim();
                if (!string.IsNullOrWhiteSpace(raw))
                    result.Add((raw, int.MinValue));
            }
            // Удаляем дубликаты, сохраняем порядок (а числовые сортируем внутри префикса, если хочешь)
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var normalized = new List<string>();

            foreach (var (p, n) in result)
            {
                var token = n == int.MinValue ? p : $"{p}{n.ToString(CultureInfo.InvariantCulture)}";
                if (seen.Add(token))
                    normalized.Add(token);
            }

            return normalized;
        }

        /// <summary>
        /// Делит designators (строкой) на N частей по количеству обозначений.
        /// Возвращает список частей в виде списков токенов.
        /// </summary>
        public static IReadOnlyList<IReadOnlyList<string>> Split(string? input, int parts)
        {
            if (parts < 2) throw new ArgumentOutOfRangeException(nameof(parts), "parts must be >= 2.");

            var tokens = Expand(input);
            if (tokens.Count == 0)
                return Enumerable.Range(0, parts).Select(_ => (IReadOnlyList<string>)Array.Empty<string>()).ToList();

            // Балансируем: первые части получают на 1 элемент больше, если не делится ровно
            var result = new List<IReadOnlyList<string>>(parts);

            int total = tokens.Count;
            int baseSize = total / parts;
            int rest = total % parts;

            int index = 0;
            for (int i = 0; i < parts; i++)
            {
                int size = baseSize + (i < rest ? 1 : 0);
                if (size <= 0) size = 0;

                var chunk = tokens.Skip(index).Take(size).ToArray();
                result.Add(chunk);
                index += size;
            }

            return result;
        }

        public static string Compress(IEnumerable<string> tokens)
        {
            if (tokens is null) return string.Empty;

            // Парсим токены в (prefix, number) или raw (number = int.MinValue)
            var parsed = new List<(string Prefix, int Number, string Raw)>();

            foreach (var t in tokens.Where(x => !string.IsNullOrWhiteSpace(x)).Select(x => x.Trim()))
            {
                if (TryParseToken(t, out var p, out var n))
                    parsed.Add((p, n, t));
                else
                    parsed.Add((t, int.MinValue, t)); // нестандартный — оставляем как есть
            }

            // Группируем по префиксу, но raw-элементы (Number=int.MinValue) выводим отдельно
            var rawItems = parsed.Where(x => x.Number == int.MinValue).Select(x => x.Raw).ToList();

            var numericGroups = parsed
                .Where(x => x.Number != int.MinValue)
                .GroupBy(x => x.Prefix, StringComparer.OrdinalIgnoreCase)
                .ToList();

            var parts = new List<string>();

            // Сначала обрабатываем числовые группы по каждому префиксу
            foreach (var g in numericGroups.OrderBy(x => x.Key, StringComparer.OrdinalIgnoreCase))
            {
                var prefix = g.Key;

                // уникальные номера, отсортированные
                var numbers = g.Select(x => x.Number).Distinct().OrderBy(x => x).ToList();
                if (numbers.Count == 0) continue;

                int i = 0;
                while (i < numbers.Count)
                {
                    int start = numbers[i];
                    int end = start;

                    int j = i + 1;
                    while (j < numbers.Count && numbers[j] == end + 1)
                    {
                        end = numbers[j];
                        j++;
                    }

                    int len = j - i;

                    if (len >= 3)
                    {
                        parts.Add($"{prefix}{start}-{prefix}{end}");
                    }
                    else if (len == 2)
                    {
                        parts.Add($"{prefix}{start}");
                        parts.Add($"{prefix}{end}");
                    }
                    else // len == 1
                    {
                        parts.Add($"{prefix}{start}");
                    }

                    i = j;
                }
            }



        // Добавляем raw-элементы (если были)
        // Можно решить порядок: либо в конец, либо по месту. Я кладу в конец (стабильнее).
        parts.AddRange(rawItems);

            return string.Join(",", parts);
        }

        /// <summary>
        /// Упаковывает список токенов обратно в строку.
        /// По умолчанию делает "D1,D2,D3". При желании можно потом добавить режим "сжимать в диапазоны".
        /// </summary>
        public static string Join(IEnumerable<string> tokens)
        {
            if (tokens is null) return string.Empty;
            return string.Join(",", tokens.Where(t => !string.IsNullOrWhiteSpace(t)).Select(t => t.Trim()));
        }
        
        public static string Compress(string? input)
        {
            var extendedd = Expand(input);
            return Compress(extendedd);
        }

        // ----------------- parsing helpers -----------------

        private static IEnumerable<string> SplitParts(string input)
        {
            // Разделители: запятая/точка с запятой/пробелы вокруг
            return input.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                        .Select(x => x.Trim())
                        .Where(x => x.Length > 0);
        }

        private static bool TryParseToken(string part, out string prefix, out int number)
        {
            prefix = string.Empty;
            number = 0;

            var m = TokenRegex.Match(part);
            if (!m.Success) return false;

            prefix = m.Groups[1].Value.Trim();
            number = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);
            return true;
        }

        private static bool TryParseRange(string part, out string prefix, out int start, out int end)
        {
            prefix = string.Empty;
            start = end = 0;

            var m = RangeRegex.Match(part);
            if (!m.Success) return false;

            var p1 = m.Groups[1].Value.Trim();
            var n1 = int.Parse(m.Groups[2].Value, CultureInfo.InvariantCulture);

            var p2 = m.Groups[3].Success ? m.Groups[3].Value.Trim() : p1; // если "D1-4"
            var n2 = int.Parse(m.Groups[4].Value, CultureInfo.InvariantCulture);

            if (!string.Equals(p1, p2, StringComparison.OrdinalIgnoreCase))
                return false; // диапазон с разным префиксом не поддерживаем

            prefix = p1;
            start = n1;
            end = n2;
            return true;
        }
    }

}

