using System.Collections.Generic;
using System.Text.Json;
using FileConverterApp.Core;

namespace FileConverterApp.Converters
{
    public class JsonConverter : IConverter
    {
        public string OutputFileExtension => "json"; // Implementación de la propiedad

        public string Convert(List<Dictionary<string, string>> data)
        {
            return JsonSerializer.Serialize(data, new JsonSerializerOptions { WriteIndented = true });
        }
    }
}