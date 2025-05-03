using System.Collections.Generic;

namespace FileConverterApp.Models
{
    public class FileConfiguration
    {
        // Cambiar de FieldDefinition a Field para incluir las propiedades de búsqueda
        // Asegúrate de que el archivo Models/Field.cs exista y tenga las propiedades
        // Name, Length, RequiresLookup, y LookupTable.
        public required List<Field> Fields { get; set; }
    }
}