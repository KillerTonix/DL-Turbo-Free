using System.Text.RegularExpressions;

namespace DL_Turbo_Free.Helper
{
    public static class RegexCache
    {
        private static Regex? _cachedActorRegex;
        private static string _lastPattern = string.Empty;

        public static Regex GetActorRegex()
        {
            string currentPattern = GlobalSettings.Default.SelectedSeparatorType;

            // Return cached version if pattern hasn't changed
            if (_cachedActorRegex != null && _lastPattern == currentPattern)
                return _cachedActorRegex;

            _lastPattern = currentPattern;

            // Use the generator logic we wrote earlier
            _cachedActorRegex = CustomRegex.CreateCustomActorRegex(currentPattern);
            return _cachedActorRegex;
        }
    }
}
