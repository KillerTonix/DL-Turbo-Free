using DL_Turbo_Free.Models;
using System.IO;
using System.Text;

namespace DL_Turbo_Free.Helper
{
    internal class AssToSrt
    {
        public static void ConvertAssToSrt(string outputPath, List<SubtitleItem> lines, bool isActorInSeparateFiles)
        {

            if (isActorInSeparateFiles)
            {
                var actors = lines.Select(x => x.Actor).Distinct();
                foreach (var actor in actors)
                {
                    StringBuilder srtLine = new();
                    var actorLines = lines.Where(x => x.Actor == actor).ToList();
                    var actorOutputPath = Path.Combine(Path.GetDirectoryName(outputPath) ?? string.Empty, $"{Path.GetFileNameWithoutExtension(outputPath)}_{actor}{Path.GetExtension(outputPath)}");
                    srtLine.AppendLine("");
                    for (int i = 0; i < actorLines.Count; i++)
                    {
                        srtLine.AppendLine($"{i + 1}");
                        srtLine.AppendLine($"{FormatTimeForSrt(actorLines[i].StartTime)} --> {FormatTimeForSrt(actorLines[i].EndTime)}");
                        srtLine.AppendLine($"{actorLines[i].Text}");
                        srtLine.AppendLine("");
                    }
                    File.WriteAllText(actorOutputPath, srtLine.ToString(), Encoding.UTF8);
                }
            }
            else
            {
                StringBuilder srtLine = new();
                srtLine.AppendLine("");
                for (int i = 0; i < lines.Count; i++)
                {
                    srtLine.AppendLine($"{i + 1}");
                    srtLine.AppendLine($"{FormatTimeForSrt(lines[i].StartTime)} --> {FormatTimeForSrt(lines[i].EndTime)}");
                    srtLine.AppendLine($"{DefinedActorSeparator(lines[i].Actor)} {lines[i].Text}");
                    srtLine.AppendLine("");
                }
                File.WriteAllText(outputPath, srtLine.ToString(), Encoding.UTF8);
            }
        }

        private static string DefinedActorSeparator(string actor)
        {
            string separator = GlobalSettings.Default.SelectedSeparatorType;
            if (string.IsNullOrWhiteSpace(actor))
                return string.Empty;
            return separator.Replace("actor", actor);
        }

        private static string FormatTimeForSrt(string time)
        {
            if (!TimeSpan.TryParse(time, out TimeSpan timeSpan))
                return "00:00:00,000";
            return string.Format("{0:D2}:{1:D2}:{2:D2},{3:D3}", (int)timeSpan.TotalHours, timeSpan.Minutes, timeSpan.Seconds, timeSpan.Milliseconds);
        }
    }
}