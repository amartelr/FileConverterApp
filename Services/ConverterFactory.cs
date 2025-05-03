using FileConverterApp.Core;
using FileConverterApp.Converters;
using System;

namespace FileConverterApp.Services
{
    public class ConverterFactory
    {
        public IConverter CreateConverter(string format)
        {
            switch (format.ToLower())
            {
                case "json":
                    return new JsonConverter();
                case "xml":
                    return new XmlConverter();
                default:
                    throw new ArgumentException($"Invalid output format: {format}");
            }
        }
    }
}