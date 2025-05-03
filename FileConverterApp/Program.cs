using ConsoleApp1.Core;
using System.Text.Json; // Needed for JSON deserialization
using ConsoleApp1.Models;
using ConsoleApp1.Services;
using ConsoleApp1.Converters; // Assuming XmlConverter is here

namespace ConsoleApp1
{
    class Program
    {
        static void Main(string[] args)
        {
            // Determine the base directory (project root)
            string baseDirectory = AppContext.BaseDirectory;
            // Navigate up to the desired project root if necessary (adjust as needed)
            // For example, if running from bin/Debug/netX.Y
            baseDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));

            // Paths relative to the determined base directory
            string inputDirectory = Path.Combine(baseDirectory, "input");
            string outputDirectory = Path.Combine(baseDirectory, "output");
            string cfgDirectory = Path.Combine(baseDirectory, "cfg"); // Define cfg directory path
            string outputFormat = "xml"; // Hardcoded to XML for now

            // Ensure output and cfg directories exist
            Directory.CreateDirectory(outputDirectory); // Ensure output directory exists
            Directory.CreateDirectory(cfgDirectory); // Ensure cfg directory exists

            var fileReaderFactory = new FileReaderFactory();
            // Assuming you want XML output based on previous context
            IConverter converter = new XmlConverter();

            // Process each file in the input directory
            if (!Directory.Exists(inputDirectory))
            {
                Console.WriteLine($"Error: Input directory not found at '{inputDirectory}'");
                return;
            }

            foreach (var inputFile in Directory.GetFiles(inputDirectory))
            {
                Console.WriteLine($"Processing file: {inputFile}");
                try
                {
                    // Pass necessary directories and factories to ProcessFile
                    ProcessFile(inputFile, outputDirectory, cfgDirectory, fileReaderFactory, converter);
                }
                catch (FileNotFoundException ex) // Specific catch for missing config or lookup file
                {
                     Console.WriteLine($"Error for {inputFile}: {ex.Message}");
                }
                catch (JsonException ex) // Specific catch for invalid JSON in config or lookup
                { // Catch errors during JSON deserialization
                     Console.WriteLine($"Invalid JSON configuration related to {inputFile}: {ex.Message}");
                }
                catch (Exception ex) // Catch-all for other processing errors
                {
                    Console.WriteLine($"An unexpected error occurred while processing {inputFile}: {ex.Message}");
                    // Optional: Log stack trace for debugging
                    // Console.WriteLine(ex.StackTrace);
                }
            }

            Console.WriteLine("Processing complete.");
        }

        // Updated signature to include cfgDir
        static void ProcessFile(string inputFile, string outputDir, string cfgDir, FileReaderFactory fileReaderFactory, IConverter converter)
        {
            // --- Configuration Loading from JSON ---
            string baseInputFileName = Path.GetFileNameWithoutExtension(inputFile); // e.g., "emp"
            string configFileName = baseInputFileName + ".json"; // e.g., "emp.json"
            string configFilePath = Path.Combine(cfgDir, configFileName);

            if (!File.Exists(configFilePath))
            {
                // Throw specific exception if config file is missing
                throw new FileNotFoundException($"Configuration file not found: '{configFilePath}'");
            }

            string jsonConfig = File.ReadAllText(configFilePath);
            // Deserialize JSON config file. Assumes FileConfiguration class exists in Models namespace.
            FileConfiguration config = JsonSerializer.Deserialize<FileConfiguration>(jsonConfig, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

            if (config == null || config.Fields == null || config.Fields.Count == 0) // Basic validation
            {
                 throw new JsonException($"Configuration file '{configFilePath}' is empty, invalid, or does not contain 'Fields'.");
            }

            // --- Process Fields for Lookups (*) ---
            foreach (var field in config.Fields)
            {
                // Ensure Name is not null before checking StartsWith
                if (field.Name != null && field.Name.StartsWith("*"))
                {
                    field.RequiresLookup = true;
                    string originalName = field.Name; // e.g., "*Codigo"
                    field.Name = field.Name.Substring(1); // Remove *, Name becomes "Codigo"

                    // Construct lookup file path: e.g., cfg/emp_Codigo.json
                    string lookupFileName = $"{baseInputFileName}_{field.Name}.json";
                    string lookupFilePath = Path.Combine(cfgDir, lookupFileName);

                    if (!File.Exists(lookupFilePath))
                    {
                        // Throw specific error if lookup file is missing
                        throw new FileNotFoundException($"Lookup file not found for field '{originalName}': Expected '{lookupFilePath}'");
                    }

                    try
                    {
                        string jsonLookup = File.ReadAllText(lookupFilePath);
                        // Deserialize the simple key-value JSON into the dictionary
                        // Ensure the lookup JSON is a simple object like {"key1":"value1", "key2":"value2"}
                        field.LookupTable = JsonSerializer.Deserialize<Dictionary<string, string>>(jsonLookup, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                        if (field.LookupTable == null)
                        {
                             throw new JsonException($"Lookup file '{lookupFilePath}' deserialized to null.");
                        }
                    }
                    catch (JsonException ex)
                    {
                        // Provide more context in the exception message
                        throw new JsonException($"Invalid JSON in lookup file '{lookupFilePath}' for field '{originalName}': {ex.Message}", ex);
                    }
                    catch (Exception ex) // Catch other potential errors during lookup file processing
                    {
                         throw new Exception($"Error processing lookup file '{lookupFilePath}' for field '{originalName}': {ex.Message}", ex);
                    }
                }
            }
             // --- End Configuration Loading ---

             // --- File Reading ---
             // Create Reader based on file extension
             IFileReader fileReader = fileReaderFactory.CreateReader(inputFile);
             // Read Data using the processed config (with lookup info)
             List<Dictionary<string, string>> data = fileReader.Read(inputFile, config); // Pass config to reader

             // --- Data Conversion ---
             // Convert Data (Converter is already created and passed in)
             string outputContent = converter.Convert(data);

             // --- File Writing ---
             // Determine output file path
             string outputFileName = Path.GetFileNameWithoutExtension(inputFile) + "." + converter.OutputFileExtension; // Access the property instead of calling a method
             string outputFile = Path.Combine(outputDir, outputFileName);

             // Write the converted content to the output file
             File.WriteAllText(outputFile, outputContent);

             Console.WriteLine($"Successfully processed '{inputFile}' and created '{outputFile}'");
        }
    }
}
