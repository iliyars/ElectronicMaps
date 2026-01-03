using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Wordprocessing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ElectronicMaps.Documents.Rendering.OpenXml
{
    /// <summary>
    /// Клонирует таблицы в Word документе для создания множественных страниц
    /// </summary>
    public class TableCloner
    {
        /// <summary>
        /// Находит таблицу-шаблон по SDT Tag и клонирует её N раз
        /// </summary>
        /// <param name="container">Контейнер (обычно Body документа)</param>
        /// <param name="templateTableTag">SDT Tag таблицы-шаблона (например, "TemplateTable")</param>
        /// <param name="pageCount">Количество копий таблицы</param>
        /// <returns>Список клонированных таблиц</returns>
        public List<Table> CloneTemplateTable(OpenXmlElement container, string templateTableTag, int pageCount)
        {
            if (container == null)
                throw new ArgumentNullException(nameof(container));
            if (string.IsNullOrWhiteSpace(templateTableTag))
                throw new ArgumentException("Template table tag cannot be empty", nameof(templateTableTag));
            if (pageCount <= 0)
                throw new ArgumentException("Page count must be > 0", nameof(pageCount));

            // Ищем таблицу, обёрнутую в SDT с нужным тегом
            var templateTableSdt = container.Descendants<SdtElement>()
                .FirstOrDefault(sdt => GetSdtTag(sdt) == templateTableTag);

            if (templateTableSdt == null)
                throw new InvalidOperationException($"Template table with tag '{templateTableTag}' not found");

            // Извлекаем саму таблицу из SDT
            var templateTable = templateTableSdt.Descendants<Table>().FirstOrDefault();
            if (templateTable == null)
                throw new InvalidOperationException($"No table found inside SDT '{templateTableTag}'");

            var clonedTables = new List<Table>();

            // Первая таблица - это сам шаблон (оставляем как есть)
            clonedTables.Add(templateTable);

            // Клонируем таблицу нужное количество раз
            var insertAfter = templateTableSdt as OpenXmlElement;

            for (int i = 1; i < pageCount; i++)
            {
                // Глубокое клонирование таблицы
                var clonedTable = (Table)templateTable.CloneNode(true);

                // Добавляем разрыв страницы перед новой таблицей
                var pageBreakPara = CreatePageBreakParagraph();

                // Вставляем после предыдущего элемента
                container.InsertAfter(pageBreakPara, insertAfter);
                container.InsertAfter(clonedTable, pageBreakPara);

                clonedTables.Add(clonedTable);
                insertAfter = clonedTable;
            }

            return clonedTables;
        }

        /// <summary>
        /// Находит все таблицы в документе по маркеру
        /// </summary>
        public List<Table> FindTablesByMarker(OpenXmlElement container, string markerTag)
        {
            return container.Descendants<SdtElement>()
                .Where(sdt => GetSdtTag(sdt) == markerTag)
                .SelectMany(sdt => sdt.Descendants<Table>())
                .ToList();
        }

        /// <summary>
        /// Создаёт параграф с разрывом страницы
        /// </summary>
        private static Paragraph CreatePageBreakParagraph()
        {
            return new Paragraph(
                new Run(
                    new Break() { Type = BreakValues.Page }
                )
            );
        }

        /// <summary>
        /// Получает Tag из SdtProperties
        /// </summary>
        private static string? GetSdtTag(SdtElement sdt)
        {
            return sdt.SdtProperties?.GetFirstChild<Tag>()?.Val?.Value;
        }

        /// <summary>
        /// Удаляет SDT обёртку, оставляя только содержимое
        /// Полезно, если нужно убрать Content Control после заполнения
        /// </summary>
        public void UnwrapSdt(SdtElement sdt)
        {
            if (sdt == null) return;

            var parent = sdt.Parent;
            if (parent == null) return;

            // Извлекаем содержимое SDT (используем общий базовый тип OpenXmlElement)
            OpenXmlElement? content = sdt.Descendants<SdtContentBlock>().FirstOrDefault();
            if (content == null)
            {
                content = sdt.Descendants<SdtContentRun>().FirstOrDefault();
            }

            if (content != null)
            {
                // Перемещаем всех детей из content в parent
                var children = content.ChildElements.ToList();
                foreach (var child in children)
                {
                    parent.InsertBefore(child.CloneNode(true), sdt);
                }
            }

            // Удаляем сам SDT
            sdt.Remove();
        }
    }
}