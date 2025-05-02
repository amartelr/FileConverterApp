using ConsoleApp1.Core;
using ConsoleApp1.Converters;
using System;

namespace ConsoleApp1.Services
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