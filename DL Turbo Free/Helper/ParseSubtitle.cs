using DL_Turbo_Free.Services;
using System.Windows;

namespace DL_Turbo_Free.Helper
{
    class ParseSubtitle
    {
        private static readonly SrtService _srtService = new();
        private static readonly AssService _AssService = new();

        public static byte[] ParseSrt(string filePath)
        {
            try { byte[] _memoryJsonData = _srtService.ConvertSrtToJsonInMemory(filePath); return _memoryJsonData; }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); return []; }
        }

        public static byte[] ParseAss(string filePath)
        {
            try { byte[] _memoryJsonData = _AssService.ConvertAssToJsonInMemory(filePath); return _memoryJsonData; }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); return []; }
        }
    }
}
