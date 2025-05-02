using ConsoleApp1.Core;
using ConsoleApp1.Readers;
using System;
using System.IO;

namespace ConsoleApp1.Services
{
    public class FileReaderFactory
    {
        public IFileReader CreateReader(string inputFile)
        {
            string extension = Path.GetExtension(inputFile).ToLower();

            switch (extension)
            {
                case ".csv":
                    return new CsvFileReader();
                default:
                    return new FixedLengthFileReader();
            }
        }
    }
}