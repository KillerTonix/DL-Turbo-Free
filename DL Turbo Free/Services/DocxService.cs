using DL_Turbo_Free.Models;
using System.IO;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace DL_Turbo_Free.Services
{
    public class DocxService
    {
        private readonly Border _blankBorder = new(BorderStyle.Tcbs_none, 0, 0f, Xceed.Drawing.Color.White);
        private readonly Border _grayBorder = new(BorderStyle.Tcbs_single, BorderSize.two, 0f, Xceed.Drawing.Color.LightGray);
        private readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;

        public void GenerateDocx(string outputPath, List<ScriptLine> lines)
        {
            string filename = Path.GetFileNameWithoutExtension(outputPath);
            // 1. Setup Paths


            // 2. Load Template
            string templatePath = Path.Combine(AppPath, "blank_pattern.docx");
            // Fallback if template is missing, create new
            using var doc = File.Exists(templatePath) ? DocX.Load(templatePath) : DocX.Create(outputPath);

            // 3. Add Title
            var p = doc.InsertParagraph(filename);
            p.Alignment = Alignment.center;
            p.FontSize(16).Font("Arial").Bold();

            // 4. Add Actor Header (List of unique actors)
            var uniqueActors = lines.Select(x => x.Actor).Distinct().OrderBy(x => x).ToList();
            var pActors = doc.InsertParagraph("Актёры: ");
            pActors.FontSize(11).Font("Arial");

            foreach (var actor in uniqueActors)
            {
                var run = pActors.Append(actor);
                pActors.Append(", ");
            }
            pActors.Append("\n");

            // 5. Main Script Table
            var table = doc.AddTable(lines.Count, 3);
            table.SetColumnWidth(0, 60);
            table.SetColumnWidth(1, 110);
            table.SetColumnWidth(2, 380);
            table.AutoFit = AutoFit.Contents;

            for (int i = 0; i < lines.Count; i++)
            {
                var line = lines[i];

                // Col 0: Time (Normal text)
                // We use the fluent chain: Append -> Font -> Size
                table.Rows[i].Cells[0].Paragraphs.First()
                    .Append(line.Timestamp).Font("Arial").FontSize(12);

                FormatBorders(table.Rows[i].Cells[0], Alignment.left);

                // Col 1: Actor (With Highlight Logic)
                var pActor = table.Rows[i].Cells[1].Paragraphs.First();

                // We append the text, which returns the 'Paragraph' object focusing on this text
                pActor.Append(line.Actor).Font("Arial").FontSize(12).Bold();



                FormatBorders(table.Rows[i].Cells[1], Alignment.right);

                // Col 2: Text (Normal text)
                table.Rows[i].Cells[2].Paragraphs.First()
                    .Append(line.Text).Font("Arial").FontSize(12);

                FormatBorders(table.Rows[i].Cells[2], Alignment.left);
            }

            doc.InsertTable(table);
            // 6. Statistics Section
            AddStatistics(doc, lines);

            // 7. Footers
            AddFooters(doc, filename);

            // 8. Save
            doc.SaveAs(outputPath);
        }

        // --- HELPER METHODS ---              


        private void FormatBorders(Cell c, Alignment align)
        {
            // Apply alignment to the text
            c.Paragraphs.First().Alignment = align;

            // Apply borders
            // Note: Use 'TableCellBorderType' from the Xceed namespace
            c.SetBorder(TableCellBorderType.Top, _blankBorder);
            c.SetBorder(TableCellBorderType.Bottom, _grayBorder);

            if (align == Alignment.right)
            {
                // Right-aligned cells (Actors) usually have no side borders in your design
                c.SetBorder(TableCellBorderType.Left, _blankBorder);
                c.SetBorder(TableCellBorderType.Right, _blankBorder);
            }
            else
            {
                // Left-aligned cells have gray sides
                c.SetBorder(TableCellBorderType.Left, _grayBorder);
                c.SetBorder(TableCellBorderType.Right, _grayBorder);
            }
        }

        private void AddStatistics(DocX doc, List<ScriptLine> lines)
        {
            var p = doc.InsertParagraph("\n\nСТАТИСТИКА ПО КОЛИЧЕСТВУ РЕПЛИК\n");
            p.Alignment = Alignment.center;
            p.FontSize(16).Font("Arial").Bold().UnderlineStyle(UnderlineStyle.singleLine);


            // OPTIONAL: Add .Trim() to ensure "John" and "John " are counted as the same actor
            var stats = lines.Where(x => !string.IsNullOrWhiteSpace(x.Actor)) // Filter empty actors
                             .GroupBy(x => x.Actor.Trim())
                             .Select(g => new { Actor = g.Key, Count = g.Count() })
                             .OrderByDescending(x => x.Count)
                             .ToList();


            // FIX 1: Add +1 to the row count to make room for the Header
            var table = doc.AddTable(stats.Count + 1, 2);
            table.AutoFit = AutoFit.Contents;

            // Header row (Row 0)
            table.Rows[0].Cells[0].Paragraphs.First().Append("Актёр").Font("Arial").FontSize(14).Bold().Alignment = Alignment.center;
            table.Rows[0].Cells[1].Paragraphs.First().Append("Количество реплик").Font("Arial").FontSize(14).Bold().Alignment = Alignment.center;

            // Data rows
            for (int i = 0; i < stats.Count; i++)
            {
                // FIX 2: Use [i + 1] to write to the row AFTER the header
                var row = table.Rows[i + 1];

                // Actor Name column
                row.Cells[0].Paragraphs.First().Append(stats[i].Actor);
                row.Cells[0].SetBorder(TableCellBorderType.Bottom, _blankBorder);

                // Count column
                row.Cells[1].Paragraphs.First().Append(stats[i].Count.ToString()).Alignment = Alignment.center;
                row.Cells[1].SetBorder(TableCellBorderType.Bottom, _blankBorder);
            }

            doc.InsertTable(table);
        }

        private void AddFooters(DocX doc, string filename)
        {
            // 1. CRITICAL: Create the footer containers in the XML
            doc.AddFooters();

            // 2. Enable the flags for specific footers
            doc.DifferentFirstPage = true;
            doc.DifferentOddAndEvenPages = true;

            // 3. Local helper to format a specific footer
            void SetFooter(Footer f)
            {
                // Safety check: sometimes 'First' or 'Even' can still be null depending on DocX version
                if (f == null) return;

                f.InsertParagraph($"\"{filename}\"")
                 .Bold()
                 .Append("   Страница ")
                 .AppendPageNumber(PageNumberFormat.normal)
                 .Append(" из ")
                 .AppendPageCount(PageNumberFormat.normal)
                 .Alignment = Alignment.center;
            }

            // 4. Now these properties are guaranteed to be initialized (or we skip them safely)
            SetFooter(doc.Footers.First);
            SetFooter(doc.Footers.Even);
            SetFooter(doc.Footers.Odd);
        }
    }
}
