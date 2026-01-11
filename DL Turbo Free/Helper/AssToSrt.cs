using DL_Turbo_Free.Models;
using System.IO;
using System.Text;

namespace DL_Turbo_Free.Helper
{
    internal class AssToSrt
    {
        public static void ConvertAssToSrt(string outputPath, List<ScriptLine> lines, bool isActorInSeparateFiles)
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
                        srtLine.AppendLine($"{actorLines[i].RawStartTime:hh\\:mm\\:ss\\,fff} --> {actorLines[i].RawEndTime:hh\\:mm\\:ss\\,fff}");
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
                    srtLine.AppendLine($"{lines[i].RawStartTime:hh\\:mm\\:ss\\,fff} --> {lines[i].RawEndTime:hh\\:mm\\:ss\\,fff}");
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
    }
}