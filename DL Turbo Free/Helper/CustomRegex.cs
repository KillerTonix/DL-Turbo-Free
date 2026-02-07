using System.Text.RegularExpressions;

namespace DL_Turbo_Free.Helper
{
    public class CustomRegex
    {
        public static Regex CreateCustomActorRegex(string template)
        {
            // 1. Validate input
            if (string.IsNullOrWhiteSpace(template) || !template.Contains("actor"))
            {
                // Fallback to default [actor] if invalid
                return new Regex(@"\[(?<actor>.*?)\]");
            }

            // 2. Split the template by the word "actor"
            // Example: "[actor]" becomes prefix="[" and suffix="]"
            string[] parts = template.Split(["actor"], StringSplitOptions.None);
            string prefix = parts.Length > 0 ? parts[0] : "";
            string suffix = parts.Length > 1 ? parts[1] : "";

            // 3. Escape the prefix and suffix 
            // This is CRITICAL. It turns special characters like [, ], (, ), +, \ into literal matchers.
            string safePrefix = Regex.Escape(prefix);
            string safeSuffix = Regex.Escape(suffix);

            // 4. Construct the dynamic pattern
            // We sandwich the capture group (?<actor>.+?) between your escaped prefix and suffix.
            string finalPattern = $"{safePrefix}(?<actor>.+?){safeSuffix}";

            return new Regex(finalPattern, RegexOptions.Compiled);
        }
    }
}
