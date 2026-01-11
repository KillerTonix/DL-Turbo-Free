namespace DL_Turbo_Free.Models
{
    public class ScriptLine
    {
        public string Timestamp { get; set; } = string.Empty;// Formatted string (e.g., "01:25")
        public string Actor { get; set; } = string.Empty;
        public string Text { get; set; } = string.Empty;
        public TimeSpan RawStartTime { get; set; } // Kept for sorting/logic
    }
}
