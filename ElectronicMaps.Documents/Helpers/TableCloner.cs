using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Wordprocessing;

namespace ElectronicMaps.Documents.Helpers
{
    public static class TableCloner
    {
        /// <summary>
        /// Клонировать первую таблицу в документе N раз
        /// </summary>
        /// <param name="document">Word документ (WordprocessingDocument)</param>
        /// <param name="totalTablesNeeded">Сколько таблиц должно быть в итоге (включая оригинал)</param>
        /// <returns>Список всех таблиц (включая оригинал)</returns>
        /// <remarks>
        /// Например, если totalTablesNeeded = 8, а в документе уже есть 1 таблица,
        /// будет создано ещё 7 копий (итого 8 таблиц).
        /// </remarks>
        /// <example>
        /// <code>
        /// using (var doc = WordprocessingDocument.Open("template.docx", true))
        /// {
        ///     var tables = TableCloner.CloneTables(doc, totalTablesNeeded: 8);
        ///     // Теперь в документе 8 таблиц
        /// }
        /// </code>
        /// </example>
        /// <exception cref="InvalidOperationException">Если документ не содержит таблиц</exception>
        public static List<Table> CloneTables(WordprocessingDocument document, int totalTableNeeded)
        {
            var body = document.MainDocumentPart?.Document.Body;

            var existingTables = body.Elements<Table>().ToList();

            if (existingTables.Count == 0)
            {
                throw new InvalidOperationException("Документ не содержит ни одной таблицы");
            }

            var templateTable = existingTables[0];

            int tablesToCrate = totalTableNeeded - existingTables.Count;

            var allTables = new List<Table>(existingTables);

            for (int i= 0; i < tablesToCrate; i++)
            {
                var cloneTable = (Table)templateTable.CloneNode(deep: true);

                body.AppendChild(cloneTable);

                allTables.Add(cloneTable);
            }

            return allTables;
        }

        /// <summary>
        /// Клонировать первую таблицу с добавлением разрывов страниц
        /// </summary>
        /// <param name="document">Word документ</param>
        /// <param name="totalTablesNeeded">Количество таблиц</param>
        /// <param name="insertPageBreaks">Добавлять разрывы страниц между таблицами?</param>
        /// <returns>Список всех таблиц</returns>
        public static List<Table> CloneTables(WordprocessingDocument document, int totalTablesNeeded, bool insertPageBreaks)
        {
            if (!insertPageBreaks)
            {
                // Обычное клонирование без разрывов страниц
                return CloneTables(document, totalTablesNeeded);
            }

            var body = document.MainDocumentPart?.Document?.Body;
            var existingTables = body.Elements<Table>().ToList();

            var templateTable = existingTables[0];
            int tablesToCreate = totalTablesNeeded - existingTables.Count;

            var allTables = new List<Table>(existingTables);

            for(int i = 0; i < tablesToCreate; i++)
            {
                // Добавляем разрыв страницы
                var pageBreakParagraph = new Paragraph(
                    new Run(
                        new Break() { Type = BreakValues.Page }
                    ));
                body.AppendChild(pageBreakParagraph);

                // Клонируем таблицу
                var clonedTable = (Table)templateTable.CloneNode(deep: true);
                body.AppendChild(clonedTable);

                allTables.Add(clonedTable);
            }

            return allTables;

        }

        /// <summary>
        /// Получить все таблицы из документа
        /// </summary>
        /// <param name="document">Word документ</param>
        /// <returns>Список таблиц</returns>
        public static List<Table> GetTables(WordprocessingDocument document)
        {
            if (document == null)
            {
                throw new ArgumentNullException(nameof(document));
            }

            var body = document.MainDocumentPart?.Document?.Body;
            return body?.Elements<Table>().ToList() ?? new List<Table>();
        }

        /// <summary>
        /// Получить количество таблиц в документе
        /// </summary>
        /// <param name="document">Word документ</param>
        /// <returns>Количество таблиц</returns>
        public static int GetTableCount(WordprocessingDocument document)
        {
            return GetTables(document).Count;
        }

    }
}
