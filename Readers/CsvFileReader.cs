using System;
using System.Collections.Generic;
using System.IO;
using FileConverterApp.Core;
using FileConverterApp.Models;
using NLog; // Add NLog

namespace FileConverterApp.Readers
{
    public class CsvFileReader : IFileReader
    {
        // Add a logger instance
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        public List<Dictionary<string, string>> Read(string inputFile, FileConfiguration config)
        {
            List<Dictionary<string, string>> data = new List<Dictionary<string, string>>();

            // Validate configuration first
            if (config == null || config.Fields == null || config.Fields.Count == 0)
            {
                Logger.Warn($"No fields configured for CSV file '{inputFile}'. Returning empty data.");
                return data; // Return empty list if no fields are defined
            }

            try
            {
                using (StreamReader reader = new StreamReader(inputFile))
                {
                    string? headerLine = reader.ReadLine(); // Read the header line
                    if (headerLine == null)
                    {
                        Logger.Error($"CSV file '{inputFile}' is empty or has no headers.");
                        return data;
                    }

                    // Optional: Validate headers against config (can be simple count or name matching)
                    string[] headers = headerLine.Split(','); // Assuming comma delimiter
                    if (headers.Length != config.Fields.Count)
                    {
                        Logger.Warn($"Header count ({headers.Length}) in '{inputFile}' does not match configured field count ({config.Fields.Count}). Data mapping might be incorrect.");
                        // Decide if you want to proceed or stop. Proceeding assumes column order matches config order.
                    }

                    string? line;
                    int lineNumber = 1; // Start counting after header
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines

                        string[] values = line.Split(','); // Assuming comma as delimiter
                        Dictionary<string, string> record = new Dictionary<string, string>();

                        // Use the number of configured fields for iteration
                        if (values.Length != config.Fields.Count)
                        {
                            Logger.Warn($"Line {lineNumber}: Number of values ({values.Length}) does not match configured field count ({config.Fields.Count}) in '{inputFile}'. Skipping line.");
                            continue; // Skip this line
                        }

                        // Iterate based on the configuration fields, assuming CSV column order matches config order
                        for (int i = 0; i < config.Fields.Count; i++)
                        {
                            Field field = config.Fields[i];
                            string rawValue = values[i].Trim(); // Get value from the corresponding column index

                            // Use the field name from the configuration
                            // Process the value (including lookup if needed)
                            record[field.Name] = ProcessFieldValue(field, rawValue, lineNumber, inputFile);
                        }

                        data.Add(record);
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Logger.Error($"Input file not found at '{inputFile}'");
                throw; // Re-throw to be handled by the main program loop
            }
            catch (IOException ex)
            {
                Logger.Error(ex, $"Error reading file '{inputFile}'");
                // Depending on requirements, you might want to throw or return partial data
            }
            catch (Exception ex)
            {
                Logger.Error(ex, $"An unexpected error occurred while reading '{inputFile}'");
                // Depending on requirements, you might want to throw or return partial data
            }

            return data;
        }

        // Helper method similar to the one in FixedLengthFileReader
        // Consider moving this to a shared utility class if more readers need it
        private string ProcessFieldValue(Field field, string rawValue, int lineNumber, string filePath)
        {
            if (field.RequiresLookup && field.LookupTable != null)
            {
                if (field.LookupTable.TryGetValue(rawValue, out string? lookupValue))
                {
                    return lookupValue ?? string.Empty;
                }
                else
                {
                    Logger.Warn($"Line {lineNumber} in '{filePath}': Lookup key '{rawValue}' not found in table for field '{field.Name}'. Using original value.");
                    return rawValue;
                }
            }
            else
            {
                return rawValue;
            }
        }
    }
}