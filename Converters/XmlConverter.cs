using System.Collections.Generic;
using System.Xml;
using System.Xml.Serialization;
using System.IO;
using ConsoleApp1.Core; // For IConverter interface

namespace ConsoleApp1.Converters
{
    public class XmlConverter : IConverter
    {
        public string OutputFileExtension => "xml";

        public string Convert(List<Dictionary<string, string>> data)
        {
            if (data == null) return string.Empty; // Handle null input gracefully

            using (StringWriter stringWriter = new StringWriter())
            {
                // Configure XML writer settings for indentation and omitting XML declaration
                var settings = new XmlWriterSettings
                {
                    Indent = true,
                    OmitXmlDeclaration = true, // Removes <?xml version="1.0" encoding="utf-16"?>
                    ConformanceLevel = ConformanceLevel.Fragment // Useful if stringWriter is used elsewhere
                };

                using (XmlWriter writer = XmlWriter.Create(stringWriter, settings))
                {
                    // Write the root element (e.g., <Records>)
                    writer.WriteStartElement("Records");

                    // Iterate through each record (dictionary) in the data list
                    foreach (var dict in data)
                    {
                        if (dict == null) continue; // Skip null dictionaries

                        // Write the element for each record (e.g., <Record>)
                        writer.WriteStartElement("Record");

                        // Iterate through each key-value pair in the dictionary
                        foreach (var kvp in dict)
                        {
                            // Ensure keys are valid XML element names
                            // Replace invalid characters (like spaces) with valid XML name characters
                            // Use the dictionary key as the element name and the value as its content
                            try
                            {
                                // Encode the key to ensure it's a valid XML element name
                                // Handles spaces, starting numbers, etc. Might make names less readable.
                                // Consider a stricter naming convention in your config/data if possible.
                                string elementName = XmlConvert.EncodeName(kvp.Key ?? "UnknownField");

                                // Write the element (e.g., <CustomerID>...</CustomerID>)
                                // Handle null values gracefully, writing an empty element or using xsi:nil
                                if (kvp.Value == null)
                                {
                                     writer.WriteStartElement(elementName);
                                     // Optional: Add xsi:nil="true" attribute if you need to distinguish null from empty
                                     // writer.WriteAttributeString("xsi", "nil", "http://www.w3.org/2001/XMLSchema-instance", "true");
                                     writer.WriteEndElement();
                                }
                                else
                                {
                                     writer.WriteElementString(elementName, kvp.Value);
                                }
                            }
                            catch (XmlException ex)
                            {
                                Console.WriteLine($"Warning: Could not create valid XML element name from key '{kvp.Key}'. Skipping field. Error: {ex.Message}");
                                // Skip this field if the key cannot be converted to a valid XML name
                            }
                            catch (ArgumentException ex) // Catch potential errors from EncodeName
                            {
                                Console.WriteLine($"Warning: Invalid character found in key '{kvp.Key}' for XML element name. Skipping field. Error: {ex.Message}");
                            }
                        }
                        // Close the record element (e.g., </Record>)
                        writer.WriteEndElement(); // End Record
                    }
                    // Close the root element (e.g., </Records>)
                    writer.WriteEndElement(); // End Records
                } // XmlWriter is disposed here, flushing content to stringWriter

                return stringWriter.ToString();
            }
        }
    }
}
