using ElectronicMaps.Domain.DTO;
using ElectronicMaps.Domain.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace ElectronicMaps.Infrastructure.Services
{
    public class ComponentNameParser : IComponentNameParser
    {
        // Types where family is separated from name (R / C / L components).
        private static readonly string[] RlcTypes =
        {
        "Резистор",
        "Конденсатор",
        "Дроссель",
        "Индуктивность",
        };
        // Типы, у которых тип = строго первое слово, но Family == Name
        private static readonly string[] SingleWordTypes =
        {
            "Микросхема",
            "Генератор"
        };

        private static readonly List<string> KnownFamilies = new()
        {
        "К10-17-4в", "К10-17а", "К10-17б", "К10-17в",
        "К10-42", "К10-43а", "К10-43б", "К10-43в",
        "К10-43Ма", "К10-43Мб", "К10-43Мв",
        "К10-50в", "К10-79", "К10-84в",
        "ОС-К53-68", "ОСМ Р1-8МП", "Р2-105",
        "Р1-8В", "К53-67", "ОС К53-68",
        "ОС К52-18", "Р2-108Б"
        };

        // Regex 1: "<Type> <Rest>"
        private static readonly Regex TypeAndRestRegex =
            new(@"^\s*(?<type>\S+)\s+(?<rest>.+)$",
                RegexOptions.Compiled);

        // Regex 2: cut trailing TU sections (supports one or two TU)
        private static readonly Regex TuTailRegex =
            new(@"^(?<name>.+?)\s+[\p{L}\p{N}\.\-/]*ТУ.*",
                RegexOptions.Compiled | RegexOptions.CultureInvariant);

        public ParsedComponentName Parse(string rawName)
        {
            if (string.IsNullOrWhiteSpace(rawName))
                throw new ArgumentException("Component name is empty.", nameof(rawName));

            var working = rawName
                .Replace("?", string.Empty) // remove all '?' characters
                .Trim();

            // 1. Split into Type + Rest.
            var match = TypeAndRestRegex.Match(working);
            if (!match.Success)
            {
                // Fallback: cannot split into type + rest.
                return new ParsedComponentName
                {
                    Raw = rawName,
                    Type = string.Empty,
                    Family = string.Empty,
                    Name = working
                };
            }

            var firstWordType = match.Groups["type"].Value;
            var firstWordRest = match.Groups["rest"].Value;

            string type;
            string rest;

            bool isRcl = RlcTypes.Contains(firstWordType);
            bool isSingleWordType = SingleWordTypes.Contains(firstWordType);

            // 1) Если это R/C/L тип — оставляем старое поведение
            if (isRcl || isSingleWordType)
            {
                type = firstWordType;
                rest = firstWordRest;
            }
            else
            {
                // 2) Универсальная эвристика: тип = все слова до первого "обозначения"
                var tokens = working.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                int designationIndex = FindDesignationIndex(tokens);

                if (designationIndex > 0)
                {
                    type = string.Join(" ", tokens[..designationIndex]);
                    rest = string.Join(" ", tokens[designationIndex..]);
                }
                else
                {
                    // fallback: как раньше — первое слово тип, остальное имя
                    type = firstWordType;
                    rest = firstWordRest;
                }
            }



            // 2. Remove TU / normative tail if present.
            var tuMatch = TuTailRegex.Match(rest);
            if (tuMatch.Success)
                rest = tuMatch.Groups["name"].Value;

            // Final trim & cleanup of trailing punctuation.
            rest = rest.Trim().TrimEnd(',', ';');



            // 3. Branch by type.
            if (RlcTypes.Contains(type))
            {
                // For R/C/L components we separate family from the full name.
                var family = ExtractFamilyForRcl(rest);
                var name = rest;
                return new ParsedComponentName
                {
                    Raw = rawName,
                    Type = type,
                    Family = family,
                    Name = name
                };
            }
            else
            {
                // For all other types: family == name.
                var name = BuildNameFromRest(rest);

                return new ParsedComponentName
                {
                    Raw = rawName,
                    Type = type,
                    Family = name,
                    Name = name
                };
            }
        }
        /// <summary>
        /// Строим Name из rest:
        /// - если нет цифр вообще: Name = весь rest
        /// - если первый токен с цифрой стоит первым: Name = только этот токен
        /// - иначе: Name = токен_до + токен_с_цифрой (например "ОСМ 142ЕН8В").
        /// </summary>
        private static string BuildNameFromRest(string rest)
        {
            if (string.IsNullOrWhiteSpace(rest))
                return string.Empty;

            var tokens = rest.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (tokens.Length == 0)
                return string.Empty;

            int iDigit = FindDesignationIndex(tokens);

            if (iDigit < 0)
            {
                // нет цифр, возвращаем всё как есть
                return rest.Trim().TrimEnd(',', ';');
            }

            if (iDigit == 0)
            {
                // обозначение сразу, типа "A3PE600-2PQ208I Microsemi"
                return tokens[0].Trim().TrimEnd(',', ';');
            }

            // есть префикс до обозначения, типа "ОСМ 142ЕН8В ..."
            var nameTokens = new[] { tokens[iDigit - 1], tokens[iDigit] };
            return string.Join(' ', nameTokens).Trim().TrimEnd(',', ';');
        }

        /// <summary>
        /// Индекс первого токена, похожего на обозначение (содержит цифры).
        /// Возвращает -1, если ничего не найдено.
        /// </summary>
        private static int FindDesignationIndex(string[] tokens)
        {
            for (int i = 0; i < tokens.Length; i++)
            {
                var token = tokens[i];
                if (token.Any(char.IsDigit))
                    return i;
            }
            return -1;
        }


        /// <summary>
        /// Extracts family for R/C/L components.
        /// Uses known families first, then heuristic based on the first token.
        /// </summary>
        private static string ExtractFamilyForRcl(string rest)
        {
            if (string.IsNullOrWhiteSpace(rest))
                return string.Empty;

            // 1) Try to match any known family inside the string.
            var known = KnownFamilies
                .FirstOrDefault(f => rest.Contains(f, StringComparison.OrdinalIgnoreCase));

            if (known is not null)
                return known;

            // 2) Fallback: use first token and split by '-' (e.g. "К10-79-25" -> "К10-79").
            var firstSpaceIndex = rest.IndexOf(' ');
            var token = firstSpaceIndex > 0 ? rest[..firstSpaceIndex] : rest;
            token = token.Trim();

            var segments = token.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
                return $"{segments[0]}-{segments[1]}";

            return token;
        }

        private static string ExtractFamily(string type, string rest)
        {
            if (string.IsNullOrWhiteSpace(rest))
                return string.Empty;

            // First token (until first space)
            int spaceIndex = rest.IndexOf(' ');
            string token = spaceIndex > 0 ? rest[..spaceIndex] : rest;

            // Try "=" sign cleanup
            token = token.Trim();

            // 1) Try auto-detect by splitting
            var segments = token.Split('-', StringSplitOptions.RemoveEmptyEntries);

            if (segments.Length >= 2)
            {
                string auto = $"{segments[0]}-{segments[1]}";

                // If auto matches known families — perfect
                if (KnownFamilies.Any(f => auto.StartsWith(f, StringComparison.OrdinalIgnoreCase)))
                    return auto;

                // Accept auto anyway (most cases)
                return auto;
            }

            // 2) Fallback: search inside whole rest for known families
            foreach (var f in KnownFamilies)
            {
                if (rest.Contains(f, StringComparison.OrdinalIgnoreCase))
                    return f;
            }

            // 3) Fallback: token itself
            return token;

        }

    }
}
