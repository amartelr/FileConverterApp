using System.Collections.Generic;

namespace FileConverterApp.Core
{
    public interface IFileReader
    {
        List<Dictionary<string, string>> Read(string inputFile, Models.FileConfiguration config);
    }
}