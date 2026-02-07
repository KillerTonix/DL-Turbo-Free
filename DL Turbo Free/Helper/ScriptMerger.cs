using DL_Turbo_Free.Models;
using System.Text;

namespace DL_Turbo_Free.Helper
{
    public class ScriptMerger
    {
        public  List<ScriptLine> MergeScript(List<SubtitleItem> rawItems, bool isDynamic)
        {
            try
            {
                var result = new List<ScriptLine>();
                if (rawItems.Count == 0) return result;

                // Sort by time to be safe
                var sorted = rawItems.OrderBy(x => x.StartTime).ToList();

                // Buffers for merging
                var currentBuilder = new StringBuilder(sorted[0].Text);
                var currentStart = TimeSpan.Parse(sorted[0].StartTime);
                string currentActor = sorted[0].Actor;

                var currentEnd = TimeSpan.Parse(sorted[0].EndTime);

                for (int i = 1; i < sorted.Count; i++)
                {
                    var next = sorted[i];
                    TimeSpan nextStart = TimeSpan.Parse(next.StartTime);

                    // Calculate gap between End of previous and Start of current
                    double gap = (nextStart - currentEnd).TotalSeconds;
                    bool isSameActor = currentActor == next.Actor;

                    // --- MERGING RULES ---
                    if (isDynamic)
                    {
                        if (isSameActor && gap <= 2)
                        {
                            currentBuilder.Append(" . . . " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else if (isSameActor && gap > 2 && gap <= 3)
                        {
                            currentBuilder.Append(" / " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else if (isSameActor && gap > 3 && gap <= 5)
                        {
                            currentBuilder.Append($" / {nextStart:h\\:mm\\:ss} " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else if (isSameActor && gap > 5)
                        {
                            currentBuilder.Append($" // {nextStart:h\\:mm\\:ss} " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else
                        {
                            result.Add(CreateLine(currentStart, currentEnd, currentActor, currentBuilder.ToString()));
                            currentStart = nextStart;
                            currentEnd = TimeSpan.Parse(next.EndTime);
                            currentActor = next.Actor;

                            currentBuilder.Clear();
                            currentBuilder.Append(next.Text);
                        }
                    }
                    else
                    {
                        if (isSameActor && gap <= 2)
                        {
                            currentBuilder.Append(" " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else if (isSameActor && gap > 2 && gap <= 10)
                        {
                            currentBuilder.Append($" // {nextStart:h\\:mm\\:ss} " + next.Text);
                            currentEnd = TimeSpan.Parse(next.EndTime);
                        }
                        else
                        {
                            result.Add(CreateLine(currentStart, currentEnd, currentActor, currentBuilder.ToString()));
                            currentStart = nextStart;
                            currentEnd = TimeSpan.Parse(next.EndTime);
                            currentActor = next.Actor;

                            currentBuilder.Clear();
                            currentBuilder.Append(next.Text);
                        }
                    }
                }

                // Add the final remaining line
                result.Add(CreateLine(currentStart, currentEnd, currentActor, currentBuilder.ToString()));

                return result;
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
                return [];
            }
        }

        private static ScriptLine CreateLine(TimeSpan start, TimeSpan end, string actor, string text)
        {
            try
            {
                return new ScriptLine
                {
                    RawStartTime = start,
                    RawEndTime = end,
                    Timestamp = start.ToString(@"h\:mm\:ss"),
                    Actor = actor,
                    Text = text
                };
            }
            catch (Exception ex)
            {
                ExceptionMessages.ShowMessage(ex);
                return new ScriptLine();
            }
        }
    }
}
