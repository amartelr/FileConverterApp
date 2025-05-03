using System;
using System.Collections.Generic;
using System.IO;
using FileConverterApp.Core;
using FileConverterApp.Models;

namespace FileConverterApp.Readers
{
    public class CsvFileReader : IFileReader
    {
        public List<Dictionary<string, string>> Read(string inputFile, FileConfiguration config)
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

            try
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {
                    string line;
                    string[] headers = reader.ReadLine()?.Split(','); // Assuming comma as delimiter

                    if (headers == null)
                    {
                        Console.WriteLine("Error: CSV file is empty or has no headers.");
                        return data;
                    }

                    while ((line = reader.ReadLine()) != null)
                    {
                        string[] values = line.Split(','); // Assuming comma as delimiter
                        Dictionary<string, string> record = new Dictionary<string, string>();

                        if (values.Length != headers.Length)
                        {
                            Console.WriteLine("Warning: Number of values does not match number of headers.");
                            continue; // Skip this line
                        }

                        for (int i = 0; i < headers.Length; i++)
                        {
                            record[headers[i].Trim()] = values[i].Trim();
                        }

                        data.Add(record);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error reading file: {ex.Message}");
            }

            return data;
        }
    }
}