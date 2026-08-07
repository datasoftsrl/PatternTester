using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace PatternTester.Infrastructure;

[JsonSourceGenerationOptions(WriteIndented = true, PropertyNameCaseInsensitive = true)]
[JsonSerializable(typeof(ConfigurationService.ConfigurationData))]
[JsonSerializable(typeof(Dictionary<string, string>))]
public partial class ConfigurationJsonContext : JsonSerializerContext
{
}
