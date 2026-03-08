using DL_Turbo_Free.Helper;
using DL_Turbo_Free.Models;
using System.IO;
using System.Threading.Tasks.Dataflow;
using System.Windows;
using Xceed.Document.NET;
using Xceed.Words.NET;

namespace DL_Turbo_Free.Services
{
    public class DocxService
    {
        private readonly Border _blankBorder = new(BorderStyle.Tcbs_none, 0, 0f, Xceed.Drawing.Color.White);
        private readonly string AppPath = AppDomain.CurrentDomain.BaseDirectory;
        public void GenerateDocx(string outputPath, List<ScriptLine> lines)
        {
            try
            {
                TableDesign tableDesign = DocxStyleHelper.GetTableDesign();   
                string fontName = DocxStyleHelper.GetFontName();
                int fontSize = DocxStyleHelper.GetFontSize();

                string filename = Path.GetFileNameWithoutExtension(outputPath);
         
                string templatePath = Path.Combine(AppPath, "blank_pattern.docx");
                // Fallback if template is missing, create new
                using var doc = File.Exists(templatePath) ? DocX.Load(templatePath) : DocX.Create(outputPath);

                // 3. Add Title
                doc.InsertParagraph(filename).FontSize(16).Bold().Font(fontName).Alignment = Alignment.center;

                // 4. Add Actor Header (List of unique actors)
                var uniqueActors = lines.Select(x => x.Actor).Distinct().OrderBy(x => x).ToList();
                var pActors = doc.InsertParagraph("Актёры: ").Font(fontName);

                foreach (var actor in uniqueActors)
                {
                    pActors.Append(actor).Font(fontName);
                    pActors.Append(", ").Font(fontName);
                }
                pActors.Append("\n");

                // 5. Main Script Table
                var table = doc.AddTable(lines.Count + 1, 3);
                table.SetColumnWidth(0, 60);
                table.SetColumnWidth(1, 110);
                table.SetColumnWidth(2, 380);
                table.Design = tableDesign;
                table.AutoFit = AutoFit.Contents;

                // Add headers to the table
                table.Rows[0].Cells[0].Paragraphs.First().Append("Время").Bold().FontSize(12).Font(fontName).Alignment = Alignment.center;
                table.Rows[0].Cells[1].Paragraphs.First().Append("Актер").Bold().FontSize(12).Font(fontName).Alignment = Alignment.center;
                table.Rows[0].Cells[2].Paragraphs.First().Append("Текст").Bold().FontSize(12).Font(fontName).Alignment = Alignment.center;

                for (int i = 0; i < lines.Count; i++)
                {
                    var line = lines[i];
                    table.Rows[i + 1].Cells[0].Paragraphs.First().Append(DocxStyleHelper.GetTimingFormat(line.Timestamp)).Font(fontName);
                    table.Rows[i + 1].Cells[1].Paragraphs.First().Append(line.Actor).FontSize(12).Bold().Font(fontName).Alignment = Alignment.right;
                    table.Rows[i + 1].Cells[2].Paragraphs.First().Append(line.Text).FontSize(fontSize).Font(fontName);
                }

                doc.InsertTable(table);
                AddStatistics(doc, lines);
                AddFooters(doc, filename);
                doc.SaveAs(outputPath);
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
            }
        }



        private void AddStatistics(DocX doc, List<ScriptLine> lines)
        {
            try
            {
                string fontName = DocxStyleHelper.GetFontName();
                doc.InsertParagraph("\n\nСТАТИСТИКА ПО КОЛИЧЕСТВУ РЕПЛИК\n").FontSize(16).Bold().Font(fontName).UnderlineStyle(UnderlineStyle.singleLine).Alignment = Alignment.center;

                var stats = lines.Where(x => !string.IsNullOrWhiteSpace(x.Actor)) // Filter empty actors
                                 .GroupBy(x => x.Actor.Trim())
                                 .Select(g => new { Actor = g.Key, Count = g.Count() })
                                 .OrderByDescending(x => x.Count)
                                 .ToList();


                // FIX 1: Add +1 to the row count to make room for the Header
                var table = doc.AddTable(stats.Count + 1, 2);
                table.AutoFit = AutoFit.Contents;

                // Header row (Row 0)
                table.Rows[0].Cells[0].Paragraphs.First().Append("Актёр").FontSize(14).Bold().Font(fontName).Alignment = Alignment.center;
                table.Rows[0].Cells[1].Paragraphs.First().Append("Количество реплик").FontSize(14).Bold().Font(fontName).Alignment = Alignment.center;

                // Data rows
                for (int i = 0; i < stats.Count; i++)
                {
                    var row = table.Rows[i + 1];

                    // Actor Name column
                    row.Cells[0].Paragraphs.First().Append(stats[i].Actor).Font(fontName);
                    row.Cells[0].SetBorder(TableCellBorderType.Bottom, _blankBorder);

                    // Count column
                    row.Cells[1].Paragraphs.First().Append(stats[i].Count.ToString()).Font(fontName).Alignment = Alignment.center;
                    row.Cells[1].SetBorder(TableCellBorderType.Bottom, _blankBorder);
                }

                doc.InsertTable(table);
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
            }
        }

        private static void AddFooters(DocX doc, string filename)
        {
            try
            {
                string fontName = DocxStyleHelper.GetFontName();

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

                    f.InsertParagraph($"\"{filename}\"\n").Bold()
                     .Append("Страница ").AppendPageNumber(PageNumberFormat.normal).Append(" из ").AppendPageCount(PageNumberFormat.normal).Font(fontName).Alignment = Alignment.center;
                }

                // 4. Now these properties are guaranteed to be initialized (or we skip them safely)
                SetFooter(doc.Footers.First);
                SetFooter(doc.Footers.Even);
                SetFooter(doc.Footers.Odd);
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
            }
        }
    }
}
