using Xceed.Document.NET;

namespace DL_Turbo_Free.Helper
{
    internal class DocxStyleHelper
    {
        public static TableDesign GetTableDesign()
        {

            return GlobalSettings.Default.DocxTableStyleValue switch
            {
                "Default" => TableDesign.TableGrid,
                "Light Grid" => TableDesign.LightGrid,             
                "Light List" => TableDesign.LightList,
                _ => TableDesign.TableGrid, // Default to TableGrid if style not found
            };
        }

        public static int GetFontSize()
        {
            return GlobalSettings.Default.DocxFontSizeValue;
        }


        public static string GetTimingFormat(string time)
        {
            TimeSpan timeSpan = TimeSpan.Parse(time);          

            return GlobalSettings.Default.DocxTimingFormatValue switch
            {
                "hh:mm:ss (00:00:00)" => timeSpan.ToString(@"hh\:mm\:ss"),
                "h:mm:ss (0:00:00)" => timeSpan.ToString(@"h\:mm\:ss"),
                "mm:ss (00:00)" => timeSpan.ToString(@"mm\:ss"),
                "mm:ss (00:00), after hour set hh:mm:ss (0:00:00)" => timeSpan.TotalHours >= 1 ? timeSpan.ToString(@"h\:mm\:ss") : timeSpan.ToString(@"mm\:ss"),
                _ => @"h\:mm\:ss", // Default to h:mm:ss if format not found
            };
        }
    }
}
