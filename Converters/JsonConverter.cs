using System.Collections.Generic;
using System.Text.Json;
using ConsoleApp1.Core;

namespace ConsoleApp1.Converters
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