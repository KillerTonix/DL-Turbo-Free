using DL_Turbo_Free.Services;
using System.Windows;

namespace DL_Turbo_Free.Helper
{
    class ParseSubtitle
    {
        private static readonly SrtService _srtService = new();

        public static byte[] ParseSrt(string filePath)
        {
            try { byte[] _memoryJsonData = _srtService.ConvertSrtToJsonInMemory(filePath); return _memoryJsonData; }
            catch (Exception ex) { MessageBox.Show($"Error: {ex.Message}"); return []; }
        }


    }
}
