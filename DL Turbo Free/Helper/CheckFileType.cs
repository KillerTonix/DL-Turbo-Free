using System.IO;

namespace DL_Turbo_Free.Helper
{
    public class CheckFileType
    {
        public static string GetFileType(string filePath)
        {
            string extension = Path.GetExtension(filePath).ToLower();

            return extension switch
            {
                ".ass" => "ass",
                ".srt" => "srt",              
                _ => "Unknown File Type",
            };
        }

    }
}
