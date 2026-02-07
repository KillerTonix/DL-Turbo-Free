using DL_Turbo_Free.Contexts;
using DL_Turbo_Free.Helper;
using DL_Turbo_Free.Models;
using System.IO;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Text.Unicode;

namespace DL_Turbo_Free.Services
{
    public partial class SrtService
    {    
        [GeneratedRegex(@"(\d{2}:\d{2}:\d{2}[,\.]\d{3})")]
        private static partial Regex RxTiming();

        readonly Regex RxBracketActor = CustomRegex.CreateCustomActorRegex(GlobalSettings.Default.SelectedSeparatorType);

        /// <summary>
        /// Parses SRT file and returns raw UTF-8 JSON bytes (ideal for memory storage/transmission).
        /// </summary>
        public byte[] ConvertSrtToJsonInMemory(string filePath)
        {
            try
            {
                var rawLines = new List<SubtitleItem>();
                // ReadAllTextAsync is also available if you want non-blocking UI
                string fileContent = File.ReadAllText(filePath, System.Text.Encoding.UTF8);

                // Standardize newlines
                // In .NET 10, string manipulation is heavily optimized
                var blocks = Regex.Split(fileContent.Replace("\r\n", "\n"), @"\n\s*\n");

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
                    var textOnlyLines = lines.Where(l => !RxTiming().IsMatch(l) && !int.TryParse(l, out _)).ToList();


                    foreach (var line in textOnlyLines)
                    {
                        if (string.IsNullOrWhiteSpace(line)) continue;

                        string currentActor = "Undefined"; // Default
                        string currentText = line.Trim();


                        var bracketMatch = RxBracketActor.Match(line);
                        if (bracketMatch.Success)
                        {
                            currentActor = CleanActorName(bracketMatch.Groups["actor"].Value.Trim());
                            // Remove the [Name] tag from text
                            currentText = RxBracketActor.Replace(line, "").Trim();
                        }


                        // Add the item
                        // Note: We create a NEW item for every line found in the block
                        rawLines.Add(new SubtitleItem(
                            globalIdCounter++,
                            start,
                            end,
                            currentActor,
                            currentText
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
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
                return []; // Return empty byte array on failure
            }

        }
        // Helper to strip special characters from Actor names
        // Removes: [ ] ( ) / \ + and trims spaces
        private static string CleanActorName(string rawName)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(rawName)) return "Undefined";

                // Replace known delimiters with empty strings
                // You can add more symbols here if needed
                var cleaned = rawName
                    .Replace("[", "")
                    .Replace("]", "")
                    .Replace("(", "")
                    .Replace(")", "")
                    .Replace("/", "")
                    .Replace("\\", "") // Backslash
                    .Replace("+", "")
                    .Replace("*", "")
                    .Replace("-", "")
                    .Replace(":", "")
                    .Trim();

                return string.IsNullOrWhiteSpace(cleaned) ? "Undefined" : cleaned;
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
                return "Undefined"; // Fallback in case of unexpected errors
            }
        }

        public static List<SubtitleItem> FillMissingActors(List<SubtitleItem> items)
        {
            try
            {
                var processedList = new List<SubtitleItem>();

                // Default fallback if the very first line is undefined
                string lastKnownActor = "Unknown";

                foreach (var item in items)
                {
                    // Check if current actor is "Undefined" (or "0_0" from our previous fallback)
                    bool isUndefined = string.Equals(item.Actor, "Undefined", StringComparison.OrdinalIgnoreCase)
                                    || string.Equals(item.Actor, "0_0", StringComparison.OrdinalIgnoreCase);

                    if (isUndefined)
                    {
                        // Create a COPY of the item, but replace the Actor with the last known one
                        var fixedItem = item with { Actor = lastKnownActor };
                        processedList.Add(fixedItem);
                    }
                    else
                    {
                        // Found a real actor! Update our memory and keep the item as is.
                        lastKnownActor = item.Actor;
                        processedList.Add(item);
                    }
                }

                return processedList;
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
                return items; // Return original list on failure
            }
        }
    }
}
