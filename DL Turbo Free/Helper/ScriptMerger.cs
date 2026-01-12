using DL_Turbo_Free.Models;
using System.Text;

namespace DL_Turbo_Free.Helper
{
    public class ScriptMerger
    {
        public List<ScriptLine> MergeScript(List<SubtitleItem> rawItems)
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

                if (isSameActor && gap <= 2)
                {
                    // Case 1: Small gap -> Merge silently
                    currentBuilder.Append(" " + next.Text);

                    // Update the "End" time of our current merging block
                    currentEnd = TimeSpan.Parse(next.EndTime);
                }
                else if (isSameActor && gap > 2)
                {
                    // Case 2: Medium gap -> Merge with timestamp separator
                    // We add a timestamp marker before the next text

                    currentBuilder.Append($" // {nextStart:h\\:mm\\:ss} " + next.Text);

                    currentEnd = TimeSpan.Parse(next.EndTime);
                }
                else
                {
                    // Case 3: Different Actor OR Huge Gap -> STOP MERGING

                    // A. Save the accumulated buffer to the list
                    result.Add(CreateLine(currentStart, currentEnd, currentActor, currentBuilder.ToString()));

                    // B. Reset buffers to start a NEW line with the current item
                    currentStart = nextStart;
                    currentEnd = TimeSpan.Parse(next.EndTime);
                    currentActor = next.Actor;

                    currentBuilder.Clear();
                    currentBuilder.Append(next.Text);
                }
            }

            // Add the final remaining line
            result.Add(CreateLine(currentStart, currentEnd, currentActor, currentBuilder.ToString()));

            return result;
        }

        private static ScriptLine CreateLine(TimeSpan start, TimeSpan end, string actor, string text)
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
    }
}
