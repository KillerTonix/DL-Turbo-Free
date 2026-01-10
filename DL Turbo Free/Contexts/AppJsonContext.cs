using DL_Turbo_Free.Models;
using System.Text.Json.Serialization;

namespace DL_Turbo_Free.Contexts
{
    // This instructs the compiler to generate the JSON serialization code 
    // for List<SubtitleItem> right now, instead of figuring it out at runtime.
    [JsonSerializable(typeof(List<SubtitleItem>))]
    [JsonSourceGenerationOptions(WriteIndented = true, PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase)]
    public partial class AppJsonContext : JsonSerializerContext
    {
    }
}
