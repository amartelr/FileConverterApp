using System.Collections.Generic; // Required for Dictionary
using System.Text.Json.Serialization; // Required for JsonIgnore

namespace FileConverterApp.Models
{
    public class Field
    {
        // Name of the field (after removing '*' if present)
        public string Name { get; set; }

        // Length (primarily for fixed-width files)
        public int Length { get; set; }

        // --- Properties for Lookup Functionality ---

        // Flag indicating if this field's value needs replacement via lookup
        [JsonIgnore] // This property is set programmatically, not read from the main config JSON
        public bool RequiresLookup { get; set; } = false;

        // Holds the mapping table loaded from the secondary JSON file (e.g., emp_Codigo.json)
        [JsonIgnore] // This property is populated programmatically
        public Dictionary<string, string>? LookupTable { get; set; } = null;

        // You can add more properties if needed for validation, type hints, etc.
        // public string Type { get; set; } // Example: "string", "int", "date"
        // public bool IsRequired { get; set; } // Example: for validation
    }
}
