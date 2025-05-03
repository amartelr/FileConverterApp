using System;
using System.Collections.Generic;
using System.IO;
using FileConverterApp.Core;
using FileConverterApp.Models;

namespace FileConverterApp.Readers
{
    public class FixedLengthFileReader : IFileReader
    {
        public List<Dictionary<string, string>> Read(string filePath, FileConfiguration config)
        {
            var data = new List<Dictionary<string, string>>();

            if (config == null || config.Fields == null || config.Fields.Count == 0)
            {
                Console.WriteLine($"Warning: No fields configured for file '{filePath}'. Returning empty data.");
                return data; // Return empty list if no fields are defined
            }

            try
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    string? line;
                    int lineNumber = 0;
                    while ((line = reader.ReadLine()) != null)
                    {
                        lineNumber++;
                        if (string.IsNullOrWhiteSpace(line)) continue; // Skip empty lines

                        var recordData = new Dictionary<string, string>();
                        int currentPosition = 0;
                        foreach (var field in config.Fields)
                        {
                            // Basic validation for field configuration
                            if (string.IsNullOrEmpty(field.Name) || field.Length <= 0)
                            {
                                Console.WriteLine($"Warning: Invalid field configuration detected (Name: '{field.Name}', Length: {field.Length}) in config for '{filePath}'. Skipping field.");
                                continue; // Skip this invalid field definition
                            }

                            // Ensure we don't read past the end of the line
                            if (currentPosition >= line.Length)
                            {
                                // Line is shorter than expected before even starting this field
                                Console.WriteLine($"Warning line {lineNumber}: Line ended before processing field '{field.Name}' (starts at {currentPosition}).");
                                // Add placeholder or decide how to handle missing fields for the rest of the line
                                // recordData[field.Name] = null; // Example
                                break; // Stop processing fields for this line
                            }
                            if (currentPosition + field.Length > line.Length)
                            {
                                // Field extends beyond the end of the line - read only available part
                                Console.WriteLine($"Warning line {lineNumber}: Line is shorter than expected for field '{field.Name}' (expected length {field.Length} starting at {currentPosition}). Reading partial data.");
                                string partialValue = line.Substring(currentPosition).Trim();
                                recordData[field.Name] = ProcessFieldValue(field, partialValue, lineNumber); // Process the partial value
                                currentPosition = line.Length; // Move position to end of line
                                break; // Stop processing fields for this line as it ended prematurely
                            }

                            // Extract the raw value based on configured length
                            string rawValue = line.Substring(currentPosition, field.Length).Trim();

                            // Process the value (including lookup if needed)
                            recordData[field.Name] = ProcessFieldValue(field, rawValue, lineNumber);

                            // Move to the start of the next field
                            currentPosition += field.Length;
                        }
                        // Only add the record if it contains any data (might be empty if line was too short initially)
                        if (recordData.Count > 0)
                        {
                             data.Add(recordData);
                        }
                    }
                }
            }
            catch (FileNotFoundException)
            {
                Console.WriteLine($"Error: Input file not found at '{filePath}'");
                // Re-throw or handle as appropriate for your application flow
                throw;
            }
            catch (IOException ex)
            {
                Console.WriteLine($"Error reading file '{filePath}': {ex.Message}");
                // Handle IO errors
            }
            catch (Exception ex) // Catch unexpected errors during reading
            {
                Console.WriteLine($"An unexpected error occurred while reading '{filePath}': {ex.Message}");
                // Handle other errors
            }

            return data;
        }

        // Helper method to process field value, including lookup
        private string ProcessFieldValue(Field field, string rawValue, int lineNumber)
        {
            // Check if lookup is required for this field
            if (field.RequiresLookup && field.LookupTable != null)
            {
                // Attempt to find the replacement value in the lookup table
                if (field.LookupTable.TryGetValue(rawValue, out string? lookupValue))
                {
                    return lookupValue ?? string.Empty; // Use the value from the lookup table (handle potential null)
                }
                else
                {
                    // Key not found in the lookup table
                    Console.WriteLine($"Warning line {lineNumber}: Lookup key '{rawValue}' not found in table for field '{field.Name}'. Using original value.");
                    return rawValue; // Fallback: use the original value if key not found
                }
            }
            else
            {
                // No lookup required, use the raw value directly
                return rawValue;
            }
        }
    }
}
