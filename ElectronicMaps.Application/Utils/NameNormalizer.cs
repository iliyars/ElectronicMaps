using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElectronicMaps.Application.Utils
{
    public static class NameNormalizer
    {
        public static string Normalize(string name)
        {
            if(string.IsNullOrWhiteSpace(name)) return string.Empty;

            name = name.Trim();

            var sb = new StringBuilder(name.Length);

            foreach (var ch in name)
            {
                // Убираем пробелы и типичные разделители
                if (char.IsWhiteSpace(ch) || ch == '-' || ch == '–' || ch == '—')
                    continue;

                // Можно привести к одному регистру (например, верхний)
                sb.Append(char.ToUpperInvariant(ch));
            }

            return sb.ToString();
        }




    }
}
