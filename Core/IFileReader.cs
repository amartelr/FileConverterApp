using System.Collections.Generic;

namespace ConsoleApp1.Core
{
    public interface IFileReader
    {
        List<Dictionary<string, string>> Read(string inputFile, Models.FileConfiguration config);
    }
}