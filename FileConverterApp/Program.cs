﻿using FileConverterApp.Core;
using System.Text.Json; // Needed for JSON deserialization
using FileConverterApp.Models;
using FileConverterApp.Services;
using FileConverterApp.Converters; // Assuming XmlConverter is here
using Microsoft.Extensions.Configuration;
using NLog; // Needed for NLog

namespace FileConverterApp
{
    class Program
    {
        // Create a logger instance for this class
        private static readonly Logger Logger = LogManager.GetCurrentClassLogger();

        static void Main(string[] args)
        {
            // Determine the base directory (project root)
            //string baseDirectory = AppContext.BaseDirectory; // This points to bin/Debug...
            // Navigate up to the desired project root if necessary (adjust as needed)
            // For example, if running from bin/Debug/netX.Y
            // baseDirectory = Path.GetFullPath(Path.Combine(baseDirectory, "..", "..", ".."));
            // More robust way to get the application's base directory where appsettings.json resides
            // Use AppContext.BaseDirectory for a more reliable path to the application's root
            string baseDirectory = AppContext.BaseDirectory;
            // Construct the full path to nlog.config and load using the recommended method
            var nlogConfigPath = Path.Combine(baseDirectory, "nlog.config");
            LogManager.Setup().LoadConfigurationFromFile(nlogConfigPath); // Load NLog configuration

            try
            {
                Logger.Info("Application starting...");

                // --- Load Configuration ---
                var configuration = new ConfigurationBuilder()
                    .SetBasePath(baseDirectory) // Set the path where appsettings.json is located
                    .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true) // Load the main config file
                    // Add other providers if needed (e.g., environment variables, command line args)
                    .Build();

                // Paths relative to the determined base directory
                string inputDirectory = Path.Combine(baseDirectory, configuration.GetValue<string>("AppSettings:InputDirectoryName") ?? "input");
                string outputDirectory = Path.Combine(baseDirectory, configuration.GetValue<string>("AppSettings:OutputDirectoryName") ?? "output");
                string cfgDirectory = Path.Combine(baseDirectory, configuration.GetValue<string>("AppSettings:ConfigDirectoryName") ?? "cfg");
                string outputFormat = configuration.GetValue<string>("AppSettings:DefaultOutputFormat") ?? "xml"; // Get format from config

                // Ensure output and cfg directories exist
                Directory.CreateDirectory(outputDirectory);
                Directory.CreateDirectory(cfgDirectory); // Ensure cfg directory exists

                var fileReaderFactory = new FileReaderFactory();
                // Assuming you want XML output based on previous context
                IConverter converter = new XmlConverter(); // Consider making this configurable or using a factory

                // Process each file in the input directory
                if (!Directory.Exists(inputDirectory))
                {
                    Logger.Error($"Input directory not found at '{inputDirectory}'");
                    return;
                }

                Logger.Info($"Using Input Directory: {inputDirectory}");
                Logger.Info($"Using Output Directory: {outputDirectory}");
                Logger.Info($"Using Config Directory: {cfgDirectory}");

                var inputFiles = Directory.GetFiles(inputDirectory);
                Logger.Info($"Found {inputFiles.Length} file(s) to process.");

                foreach (var inputFile in inputFiles)
                {
                    Logger.Info($"Processing file: {inputFile}");
                    try
                    {
                        // Pass necessary directories and factories to ProcessFile
                        ProcessFile(inputFile, outputDirectory, cfgDirectory, fileReaderFactory, converter);
                    }
                    catch (FileNotFoundException ex) // Specific catch for missing config or lookup file
                    {
                        Logger.Error(ex, $"Error for {inputFile}: {ex.Message}"); // Log exception details
                    }
                    catch (JsonException ex) // Specific catch for invalid JSON in config or lookup
                    { // Catch errors during JSON deserialization
                        Logger.Error(ex, $"Invalid JSON configuration related to {inputFile}: {ex.Message}");
                    }
                    catch (Exception ex) // Catch-all for other processing errors
                    {
                        Logger.Error(ex, $"An unexpected error occurred while processing {inputFile}: {ex.Message}");
                        // NLog can be configured to log stack traces automatically
                    }
                }

                Logger.Info("Processing complete.");
            }
            catch (Exception ex)
            {
                // Catch any exception that happens during startup/initialization
                Logger.Fatal(ex, "Application terminated unexpectedly during setup.");
            }
            finally
            {
                // Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
                LogManager.Shutdown();
            }
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
            FileConfiguration? config = JsonSerializer.Deserialize<FileConfiguration>(jsonConfig, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

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
             string outputFileName = Path.GetFileNameWithoutExtension(inputFile) + "." + converter.OutputFileExtension;
             string outputFile = Path.Combine(outputDir, outputFileName);

             // Write the converted content to the output file
             File.WriteAllText(outputFile, outputContent);

             Logger.Info($"Successfully processed '{inputFile}' and created '{outputFile}'");
        }
    }
}
