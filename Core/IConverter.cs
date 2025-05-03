using System.Collections.Generic;

namespace FileConverterApp.Core
{
    public interface IConverter
    {
        string Convert(List<Dictionary<string, string>> data);
        string OutputFileExtension { get; } // Added property for output file extension
    }
}