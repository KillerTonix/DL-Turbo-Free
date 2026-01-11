using DL_Turbo_Free.Models;
using System.IO;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace DL_Turbo_Free.Services
{
    public partial class AssService
    {       
        // Regex to remove ASS tags like {\pos(10,10)} or {\i1}
        [GeneratedRegex(@"\{.*?\}")]
        private static partial Regex RxAssTags();

        // Regex to handle ASS line breaks (\N, \n, \h)
        [GeneratedRegex(@"\\[Nnh]")]
        private static partial Regex RxAssNewlines();

        public byte[] ConvertAssToJsonInMemory(string filePath)
        {
            var rawLines = new List<SubtitleItem>();
            string fileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);
            var lines = fileContent.Split('\n');

            int globalIdCounter = 1;

            // Standard ASS "Dialogue" format has 10 fields.
            // Format: Layer, Start, End, Style, Name, MarginL, MarginR, MarginV, Effect, Text
            // Indices:  0      1      2     3      4       5        6        7       8      9

            foreach (var line in lines)
            {
                string cleanLine = line.Trim();

                // We only care about lines starting with "Dialogue:"
                if (!cleanLine.StartsWith("Dialogue:", StringComparison.OrdinalIgnoreCase))
                    continue;

                // Remove "Dialogue: " prefix to make splitting easier
                // "0,0:00:02.39,..."
                string content = cleanLine.Substring(9).Trim();

                // CRITICAL: We split by comma, BUT we limit to 10 parts.
                // This ensures that if the TEXT contains commas, they are not split.
                var parts = content.Split(',', 10);

                if (parts.Length < 10) continue; // Invalid line

                string startRaw = parts[1].Trim();
                string endRaw = parts[2].Trim();
                string actorRaw = parts[4].Trim(); // The "Name" field
                string textRaw = parts[9].Trim();  // The "Text" field

                // 1. Clean the Text
                // Remove {tags}
                string noTags = RxAssTags().Replace(textRaw, "");
                // Replace \N with spaces (or newlines if you prefer)
                string cleanText = RxAssNewlines().Replace(noTags, " ").Trim();

                // 2. Normalize Time (ASS is h:mm:ss.cc, we want standard 00:00:00.000)
                string normStart = NormalizeAssTime(startRaw);
                string normEnd = NormalizeAssTime(endRaw);

                // 3. Add to list
                // Note: ASS files usually don't have the "Actor: Text" pattern inside the text field
                // because they have a specific "Name" column (parts[4]). 
                // So we can use actorRaw directly.

                // Handle empty actor
                if (string.IsNullOrWhiteSpace(actorRaw)) actorRaw = "Undefined";

                rawLines.Add(new SubtitleItem(
                    globalIdCounter++,
                    normStart,
                    normEnd,
                    actorRaw,
                    cleanText
                ));
            }

            // Serialize to JSON (Compatible with your Merger and Docx generator)
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
            };

            return JsonSerializer.SerializeToUtf8Bytes(rawLines, options);
        }

        // Helper to turn "0:00:02.39" into "00:00:02.390"
        private string NormalizeAssTime(string assTime)
        {
            // Add leading zero to hour if needed
            var parts = assTime.Split(':');
            if (parts[0].Length == 1) assTime = "0" + assTime;

            // ASS uses 2 decimals (.39) -> .NET wants 3 (.390) or 7.
            // We ensure it parses correctly by simple padding or using TimeSpan directly
            if (TimeSpan.TryParse(assTime, out TimeSpan ts))
            {
                return ts.ToString(@"hh\:mm\:ss\.fff");
            }
            return assTime.Replace(".", ","); // Fallback
        }
    }
}
