using System.Collections.Generic;

namespace ConsoleApp1.Core
{
    public interface IConverter
    {
        string Convert(List<Dictionary<string, string>> data);
        string OutputFileExtension { get; } // Added property for output file extension
    }
}