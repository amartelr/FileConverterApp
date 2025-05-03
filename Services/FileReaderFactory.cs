using FileConverterApp.Core;
using FileConverterApp.Readers;
using System;
using System.IO;

namespace FileConverterApp.Services
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