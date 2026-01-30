using DocumentFormat.OpenXml.Wordprocessing;

namespace ElectronicMaps.Documents.Helpers
{
    public static class CellWriter
    {

        public static bool WriteCell(Table table, int rowIndex, int columnIndex, string value)
        {
            var rows = table.Elements<TableRow>().ToList();
            var row = rows[rowIndex];

            var cells = row.Elements<TableCell>().ToList();

            var cell = cells[columnIndex];

            var paragraph = new Paragraph(new Run(new Text(value ?? "")));
            cell.Append(paragraph);

            return true;
        }

        public static string? ReadCell(Table table, int rowIndex, int columnIndex)
        {
            var rows = table.Elements<TableRow>().ToList();

            var row = rows[rowIndex];
            var cells = row.Elements<TableCell>().ToList();

            var cell = cells[columnIndex];

            var text = string.Join(" ", cell.Descendants<Text>().Select(t => t.Text));

            return string.IsNullOrEmpty(text) ? null : text.Trim();
        }

        public static bool ClearCell(Table table, int rowIndex, int columnIndex)
        {
            return WriteCell(table, rowIndex, columnIndex, string.Empty);
        }

        public static (int rowCount, int clumnCount) GetTableSize(Table table)
        {
            var rows = table.Elements<TableRow>().ToList();
            var rowCount = rows.Count;
            var columnCount = rows.FirstOrDefault()?.Elements<TableCell>().Count() ?? 0;

            return (rowCount, columnCount);
        }



    }
}
