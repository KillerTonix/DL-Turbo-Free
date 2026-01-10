using DL_Turbo_Free.Contexts;
using DL_Turbo_Free.Models;
using System.IO;
using System.Reflection.Metadata.Ecma335;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace DL_Turbo_Free.Services
{
    public partial class SrtService
    {
        // .NET 7+ / .NET 10 Feature: Source Generated Regex
        // This generates optimized parsing code at compile time, avoiding runtime interpretation overhead.
        [GeneratedRegex(@"(\d{2}:\d{2}:\d{2}[,\.]\d{3})")]
        private static partial Regex RxTiming();

        [GeneratedRegex(@"\[(?<actor>.*?)\]|\((?<actor>.*?)\)|(?<actor>[^\s]+)[\\/+\)]")]
        private static partial Regex RxActor();

        /// <summary>
        /// Parses SRT file and returns raw UTF-8 JSON bytes (ideal for memory storage/transmission).
        /// </summary>
        public byte[] ConvertSrtToJsonInMemory(string filePath)
        {
            var rawLines = new List<SubtitleItem>();
            // ReadAllTextAsync is also available if you want non-blocking UI
            string fileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

            // Standardize newlines
            // In .NET 10, string manipulation is heavily optimized
            var blocks = fileContent.Replace("\r\n", "\n").Split("\n\n");

            int globalIdCounter = 1;

            foreach (var block in blocks)
            {
                if (string.IsNullOrWhiteSpace(block)) continue;

                var lines = block.Split('\n');
                if (lines.Length < 2) continue;

                // Use the Source Generated Regex
                var timeMatches = RxTiming().Matches(block);
                if (timeMatches.Count < 2) continue;

                string start = timeMatches[0].Value.Replace(",", ".");
                string end = timeMatches[1].Value.Replace(",", ".");

                // Extract text lines (skipping ID and Timestamp)
                // leveraging modern LINQ optimization
                var textLines = lines.Where(l =>
                    !RxTiming().IsMatch(l) &&
                    !int.TryParse(l, out _)
                );

                string rawText = string.Join(" ", textLines);
                string cleanText = RxActor().Replace(rawText, "").Trim();
                if (cleanText.StartsWith(':')) cleanText = cleanText[1..].Trim();

                var actorMatches = RxActor().Matches(rawText);

                if (actorMatches.Count > 0)
                {
                    foreach (Match match in actorMatches)
                    {
                        // MAGIC HERE: We use the named group "actor"
                        // This automatically picks the correct name whether it was [Name] or Name/
                        string extractedActor = match.Groups["actor"].Value;

                        rawLines.Add(new SubtitleItem(
                            globalIdCounter++,
                            start,
                            end,
                            extractedActor,
                            cleanText
                        ));
                    }
                }
                else
                {
                    // No actor found
                    rawLines.Add(new SubtitleItem(
                        globalIdCounter++,
                        start,
                        end,
                        "Undefined",
                        cleanText
                    ));
                }
            }

            // SERIALIZATION
            var options = new JsonSerializerOptions
            {
                WriteIndented = true,
                // This tells the serializer: "Don't escape Unicode characters (like Cyrillic)"
                Encoder = JavaScriptEncoder.Create(UnicodeRanges.All)
            };

            // 2. Initialize your Context with these custom options
            var context = new AppJsonContext(options);

            // 3. Serialize using the configured context
            return JsonSerializer.SerializeToUtf8Bytes(rawLines, context.ListSubtitleItem);
        }
    }
}
