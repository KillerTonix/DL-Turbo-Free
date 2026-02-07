using System.Runtime.CompilerServices;
using System.Windows;

namespace DL_Turbo_Free.Helper
{
    public class ExceptionMessages
    {
        public static void ShowMessage(Exception ex, [CallerMemberName] string callerName = "")
        {
            string message = $"Error: {ex.Message}";
            string stackTrace = $"Stack Trace: {ex.StackTrace}";

            string fullMessage = $"{callerName} -> {message}{Environment.NewLine}{stackTrace}";

            MessageBox.Show(fullMessage, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
