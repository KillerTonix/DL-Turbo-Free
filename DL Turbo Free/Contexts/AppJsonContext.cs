using DL_Turbo_Free.Models;
using System.Text.Json.Serialization;

namespace DL_Turbo_Free.Contexts
{
    [JsonSourceGenerationOptions(
    WriteIndented = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase,
    PropertyNameCaseInsensitive = true)] // <--- ADD THIS
    [JsonSerializable(typeof(List<SubtitleItem>))]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}
